using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;

namespace R10.Core.Services.Shared
{
    public class NumberFormatService : INumberFormatService
    {
        private const string DelimiterCheckDigit = "/-.";
        private const string Delimiter = ",.:;-/";
        private const string StartTag = "<<";
        private const string EndTag = ">>";

        private int _defaultStandardDigits = 7;

        private IWebLinksRepository _repository;
        public NumberFormatService(IWebLinksRepository repository)
        {
            _repository = repository;
        }

        public async Task<WebLinksParsedInfoDTO> StandardizeNumber(WebLinksNumberInfoDTO numberInfo)
        {
            var templates = await GetNumberTemplates(numberInfo.SystemType, numberInfo.Country, numberInfo.CaseType, numberInfo.NumberType, WebLinksTemplateType.User, "");
            var parsedInfo = ParseNumber(templates, numberInfo);
            return parsedInfo;
        }

        public async Task<List<WebLinksNumberTemplateDTO>> GetNumberTemplates(string systemType, string country,
           string caseType, string numberType, string templateType, string templateRank)
        {
             return await _repository.GetNumberTemplates(systemType, country, caseType, numberType, templateType, templateRank);
        }

        public async Task<string> FormatNumber(WebLinksNumberInfoDTO numberInfo, string templateType, string templateRank = "")
        {
            var formatTemplates = await GetNumberTemplates(numberInfo.SystemType, numberInfo.Country, numberInfo.CaseType, numberInfo.NumberType, templateType, templateRank);
            var qualifiedTemplates = GetQualifiedTemplates(formatTemplates, numberInfo.NumberType, numberInfo.Number, numberInfo.NumberDate);
            if (qualifiedTemplates.Count > 0)
            {
                var standardTemplates = await GetNumberTemplates(numberInfo.SystemType, numberInfo.Country, numberInfo.CaseType, numberInfo.NumberType, WebLinksTemplateType.User, "");
                var targetTemplate = qualifiedTemplates[0].Template;
                var formattedNumber = FormatNumber(numberInfo, standardTemplates, targetTemplate);
                return formattedNumber;
            }
            return numberInfo.Number;
        }

        public string FormatNumber(WebLinksNumberInfoDTO numberInfo, List<WebLinksNumberTemplateDTO> standardTemplates, string targetTemplate)
        {
            var number = numberInfo.Number;
            if (standardTemplates.Count > 0 && !string.IsNullOrEmpty(targetTemplate))
            {
                var parsedInfo = ParseNumber(standardTemplates, numberInfo);
                if (parsedInfo != null && parsedInfo.Success)
                {
                    number = BuildNumber(parsedInfo, numberInfo, targetTemplate);
                }
            }
            return number;
        }

        public string CleanUpNumber(string userNumber)
        {

            var isValid = false;
            string retVal = string.Empty;

            if (!string.IsNullOrEmpty(userNumber))
            {

                // -- remove all spaces
                userNumber = userNumber.Replace(" ", "");
                var len = userNumber.Length;

                // -- if last part of number is doc kind (eg. A1, B1, C1...), exclude it there should be at least 2 Chars
                var hasKd = Regex.IsMatch(userNumber, @"[A-Za-z][0-9]$");
                if (hasKd)
                {
                    len = len - 2;
                }

                for (var i = 0; i < len; i++)
                {
                    var charStr = userNumber[i];

                    // -- check if strCharacter is a number
                    if (char.IsDigit(charStr))
                    {
                        // -- add number to the return value
                        retVal += charStr;
                        isValid = true;
                    }
                    else if (char.IsLetter(charStr))
                        // add alphabet to the return value
                        retVal += charStr;

                    else if (DelimiterCheckDigit.IndexOf(charStr) > -1)
                    {

                        // -- exclude delimeter if it is the last character
                        if (i == len-1)
                            break;

                        if (retVal.Length > 0)
                        {
                            // -- check if the next character is a number (for determining if it's a check digit)
                            if (char.IsDigit(userNumber[i + 1]))
                            {
                                // check if the next character is not a number (for determining if its a check digit)
                                // (pad with 1 space to avoid error when there is only a strChar after check digit)
                                if (!char.IsDigit((userNumber + " ")[i + 2]))
                                {
                                    // add the delimeter and the check digit to the return value
                                    // delim followed by a number, then a non-numeric strChar;
                                    // ignore the rest of the string starting from the non-numeric strChar)
                                    retVal = retVal + charStr + userNumber[i + 1];
                                    i = len;

                                }
                                else
                                    retVal += charStr;

                            }
                            else
                                retVal += charStr;
                        }

                    }
                }
            }

            return isValid ? retVal : "";
        }


        public WebLinksParsedInfoDTO ParseNumber(List<WebLinksNumberTemplateDTO> standardTemplates, WebLinksNumberInfoDTO numberInfo, int digitCount = 0)
        {
            digitCount = digitCount == 0 ? _defaultStandardDigits : digitCount;
            numberInfo.Number = CleanUpNumber(numberInfo.Number);

            foreach (var template in standardTemplates)
            {
                var parsedInfo = ParseNumber(template.Template, numberInfo);
                if (parsedInfo.Success)
                    return parsedInfo;
            }
            return null;
        }

        

        protected WebLinksParsedInfoDTO ParseNumber(string template, WebLinksNumberInfoDTO numberInfo)
        {
            var skipTemplate = false;
            var posTemplate = 0;
            var posNumber = 0;

            var countTemplateChar = 0;
            var countNumberChar = 0;

            var parsedNumber = string.Empty;
            var parsedYear = string.Empty;
            var parsedCheckDigit = string.Empty;
            var parsedCity = string.Empty;
            var parsedPriorityCountry = string.Empty;


            template = template.ToUpper();
            while (!skipTemplate && posTemplate < template.Length && posNumber < numberInfo.Number.Length)
            {
                var templateChar = template[posTemplate];
                char numberChar;

                string numberChars;
                string templateChars;

                switch (templateChar)
                {
                    case 'N':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'N');
                        skipTemplate = countTemplateChar > (numberInfo.Number.Length - posNumber);

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);

                            skipTemplate = !numberChars.IsNumeric();
                            if (!skipTemplate)
                            {
                                parsedNumber = parsedNumber + numberChars;
                            }
                        }
                        break;

                    case '+':
                        countNumberChar = CountDigits(numberInfo.Number, posNumber);
                        countTemplateChar = 1;

                        numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);
                        skipTemplate = !numberChars.IsNumeric();
                        if (!skipTemplate)
                        {
                            parsedNumber = parsedNumber + numberChars;
                        }
                        break;

                    case 'Y':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'Y');
                        skipTemplate = countTemplateChar > (numberInfo.Number.Length - posNumber);

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);
                            skipTemplate = numberInfo.NumberDate == null || numberChars.Length == 0;
                            if (!skipTemplate)
                            {
                                var yearString = numberInfo.NumberDate.Value.Year.ToString();
                                if (yearString.Right(numberChars.Length) == numberChars)
                                {
                                    parsedYear = numberChars;
                                }
                                else
                                {
                                    skipTemplate = true;
                                }
                            }
                        }
                        break;

                    case 'Z':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'Z');
                        skipTemplate = (posTemplate + countTemplateChar - 1) > numberInfo.Number.Length;

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, posNumber + countNumberChar > numberInfo.Number.Length ? numberInfo.Number.Length - posNumber : countNumberChar);
                            parsedYear = numberChars;
                        }
                        break;

                    case '"':
                        templateChars = template.Substring(posTemplate + 1);
                        skipTemplate = templateChars.IndexOf('"', StringComparison.Ordinal) < 0;

                        if (!skipTemplate)
                        {
                            templateChars =
                                templateChars.Substring(0, templateChars.IndexOf('"', StringComparison.Ordinal));

                            countNumberChar = templateChars.Length;
                            countTemplateChar = countNumberChar + 2;
                            skipTemplate = (templateChars != numberInfo.Number.Substring(posNumber, countNumberChar));
                        }
                        break;

                    case 'D':
                        numberChar = numberInfo.Number[posNumber];
                        skipTemplate = (Delimiter.IndexOf(numberChar) < 0);

                        if (!skipTemplate)
                        {
                            countNumberChar = 1;
                            countTemplateChar = 1;
                        }
                        break;

                    case 'C':
                        numberChar = numberInfo.Number[posNumber];
                        skipTemplate = !char.IsDigit(numberChar);

                        if (!skipTemplate)
                        {
                            countNumberChar = 1;
                            countTemplateChar = 1;
                            parsedCheckDigit = numberChar.ToString();
                        }
                        break;

                    case 'A':
                        numberChar = numberInfo.Number[posNumber];
                        skipTemplate = !char.IsLetter(numberChar);

                        if (!skipTemplate)
                        {
                            countNumberChar = 1;
                            countTemplateChar = 1;
                        }
                        break;

                    case 'F':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'F');
                        skipTemplate = countTemplateChar > (numberInfo.Number.Length - posNumber);

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);

                            skipTemplate = numberInfo.FilDate == null || numberChars.Length == 0;
                            if (!skipTemplate)
                            {
                                var yearString = numberInfo.FilDate.Value.Year.ToString();
                                if (yearString.Right(numberChars.Length) == numberChars)
                                {
                                    parsedYear = numberChars;
                                }
                                else
                                {
                                    skipTemplate = true;
                                }
                            }
                        }

                        break;

                    case 'G':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'G');
                        skipTemplate = countTemplateChar > (numberInfo.Number.Length - posNumber);

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);

                            skipTemplate = numberInfo.IssRegDate == null || numberChars.Length == 0;
                            if (!skipTemplate)
                            {
                                var yearString = numberInfo.IssRegDate.Value.Year.ToString();
                                if (yearString.Right(numberChars.Length) == numberChars)
                                {
                                    parsedYear = numberChars;
                                }
                                else
                                {
                                    skipTemplate = true;
                                }
                            }
                        }
                        break;

                    case 'U':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'U');
                        skipTemplate = countTemplateChar > (numberInfo.Number.Length - posNumber);

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);

                            skipTemplate = numberInfo.PubDate == null || numberChars.Length == 0;
                            if (!skipTemplate)
                            {
                                var yearString = numberInfo.PubDate.Value.Year.ToString();
                                if (yearString.Right(numberChars.Length) == numberChars)
                                {
                                    parsedYear = numberChars;
                                }
                                else
                                {
                                    skipTemplate = true;
                                }
                            }
                        }
                        break;

                    case 'V':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'V');
                        skipTemplate = countTemplateChar > (numberInfo.Number.Length - posNumber);

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);

                            if (numberChars.IsAlpha())
                            {
                                parsedCity = numberChars;
                            }
                            else
                            {
                                skipTemplate = true;
                            }
                        }
                        break;

                    case 'P':
                        countTemplateChar = CountRepeatedChars(template, posTemplate, 'P');
                        skipTemplate = countTemplateChar > (numberInfo.Number.Length - posNumber);

                        if (!skipTemplate)
                        {
                            countNumberChar = countTemplateChar;
                            numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);

                            if (numberChars.IsAlpha())
                            {
                                parsedPriorityCountry = numberChars;
                            }
                            else
                            {
                                skipTemplate = true;
                            }
                        }
                        break;

                    case 'I':
                        numberChar = numberInfo.Number[posNumber];
                        skipTemplate = !char.IsDigit(numberChar);

                        if (!skipTemplate)
                        {
                            countNumberChar = 1;
                            countTemplateChar = 1;
                        }
                        break;

                    case 'X':
                        countNumberChar = 1;
                        countTemplateChar = 1;
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        skipTemplate = template[posTemplate] == 'N';
                        if (!skipTemplate)
                        {

                            countTemplateChar = CountDigits(template, posTemplate);
                            templateChars = template.Substring(posTemplate, countTemplateChar);

                            skipTemplate = !templateChars.IsNumeric();
                            if (!skipTemplate)
                            {
                                var templateInt = Convert.ToInt32(templateChars) - 1;

                                if (templateInt > 0)
                                {
                                    countNumberChar = CountDigits(numberInfo.Number, posNumber);
                                    skipTemplate = countNumberChar < 1;

                                    if (!skipTemplate)
                                    {
                                        countNumberChar = countNumberChar > templateInt ? templateInt : countNumberChar;
                                        numberChars = numberInfo.Number.Substring(posNumber, countNumberChar);

                                        skipTemplate = !numberChars.IsNumeric();
                                        if (!skipTemplate)
                                            parsedNumber = parsedNumber + numberChars;
                                    }
                                    else
                                        countNumberChar = 0;
                                }
                            }
                        }

                        break;

                    default:
                        skipTemplate = true;
                        break;
                }

                if (!skipTemplate)
                {
                    posNumber += countNumberChar;
                    posTemplate += countTemplateChar;
                }

            }

            var parsedInfo = new WebLinksParsedInfoDTO();

            if (!skipTemplate)
            {
                skipTemplate = posTemplate != template.Length || posNumber != numberInfo.Number.Length;
            }

            if (!skipTemplate)
            {
                if (numberInfo.Country == "WO" && !string.IsNullOrEmpty(parsedPriorityCountry))
                {
                    parsedInfo.Number = (parsedPriorityCountry + parsedNumber)
                        .PadLeft(_defaultStandardDigits - parsedPriorityCountry.Length).Replace(" ", "0");
                    parsedInfo.PriorityCountry = parsedPriorityCountry;
                }
                else
                    parsedInfo.Number = parsedNumber.PadLeft(_defaultStandardDigits).Replace(" ", "0");

                parsedInfo.Year = parsedYear;
                parsedInfo.CheckDigit = parsedCheckDigit;
                parsedInfo.City = parsedCity;
                parsedInfo.Template = template;
                parsedInfo.Success = true;
            }
            else
            {
                parsedInfo.Number = "Error";
            }

            return parsedInfo;
        }

        protected string BuildNumber(WebLinksParsedInfoDTO parsedInfo, WebLinksNumberInfoDTO numberInfo, string template)
        {
            string builtNumber = string.Empty;

            if (!(string.IsNullOrEmpty(parsedInfo.Number) || string.IsNullOrEmpty(template)))
            {
                string baseNumber = string.Empty;
                string baseNumberInt = parsedInfo.Number.GetConsecutiveDigits().ToString();
                int totalNumberCount = template.CountAllChars("N");
                if (totalNumberCount > baseNumberInt.Length)
                    baseNumberInt = baseNumberInt.PadLeft(totalNumberCount, '0');

                int pos = 0;
                int digitPosition = 0;
                while (pos < template.Length)
                {
                    int charCount = 0;
                    char charTemplate = template[pos];
                    int charCountYear = 0;
                    string numberChars = "";
                    string templateChars = "";

                    switch (charTemplate)
                    {
                        case 'N':
                            {
                                charCount = CountRepeatedChars(template, pos, 'N');
                                baseNumber = baseNumberInt.ToString().PadLeft(charCount).Replace(" ", "0").Substring(digitPosition, charCount);
                                builtNumber += baseNumber;
                                digitPosition += baseNumber.Length;
                                break;
                            }

                        case 'D': //same as N but formatted with comma
                            {
                                charCount = CountRepeatedChars(template, pos, 'D');
                                if (int.TryParse(baseNumberInt, out var n))
                                {
                                    baseNumber = $"{n:n0}";
                                    var commaCount = baseNumber.Count(c => c == ',');
                                    baseNumber = baseNumber.PadLeft(charCount + commaCount, '0');
                                    builtNumber += baseNumber;
                                }
                                break;
                            }

                        case 'S': //'special template like for US appno when you want to split the number like (NN/NNN,NNN)
                            {
                                charCount = CountRepeatedChars(template, pos, 'S');
                                baseNumber = parsedInfo.Number.Substring(0, charCount);
                                baseNumberInt = baseNumberInt.Substring(baseNumber.Substring(0, 1) == "0" ? 1 : 2);
                                builtNumber += baseNumber;
                                break;
                            }


                        case '+':
                            {
                                charCount = 1;
                                builtNumber += baseNumberInt.ToString().Substring(digitPosition);
                                break;
                            }

                        case '"':
                            {
                                templateChars = template.Substring(pos + 1);
                                if (templateChars.IndexOf("\"", StringComparison.Ordinal) >= 0)
                                {
                                    templateChars = templateChars.Substring(0, templateChars.IndexOf("\"", StringComparison.Ordinal));

                                    charCount = templateChars.Length + 2;
                                    builtNumber += templateChars;
                                }
                                else
                                    charCount = 1;
                                break;
                            }

                        case 'C':
                            {
                                if (!string.IsNullOrEmpty(parsedInfo.CheckDigit))
                                    numberChars = parsedInfo.CheckDigit;
                                else
                                    numberChars = GetCheckDigit(baseNumber.Length > 0 ? baseNumber : baseNumberInt, numberInfo);

                                charCount = 1;
                                builtNumber += numberChars;
                                break;
                            }

                        case 'Y':
                            {
                                charCount = CountRepeatedChars(template, pos, 'Y');
                                charCountYear = charCount >= 4 ? 4 : charCount;

                                if (!string.IsNullOrEmpty(parsedInfo.Year))
                                {
                                    //GET FOUR DIGIT YEAR WHEN
                                    //TEMPLATE REQUIRES FOUR DIGIT YEAR (YYYY)
                                    //BUT parsedInfo.Year ONLY HAS 2 DIGITS
                                    if (parsedInfo.Year.Length < charCount)
                                    {
                                        int fourDigitYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(int.Parse(parsedInfo.Year));
                                        numberChars = fourDigitYear.ToString();
                                    }
                                    else
                                        numberChars = parsedInfo.Year.Right(charCountYear);
                                }
                                else
                                    numberChars = numberInfo.NumberDate?.Year.ToString().Right(charCountYear);

                                builtNumber += numberChars;
                                break;
                            }

                        case 'F':
                            {
                                charCount = CountRepeatedChars(template, pos, 'F');
                                charCountYear = charCount > 4 ? 4 : charCount;

                                if (!string.IsNullOrEmpty(parsedInfo.Year))
                                    numberChars = parsedInfo.Year.ToString().Right(charCountYear);
                                else
                                    numberChars = numberInfo.FilDate?.Year.ToString().Right(charCountYear);

                                builtNumber += numberChars;
                                break;
                            }

                        case 'U':
                            {
                                charCount = CountRepeatedChars(template, pos, 'U');
                                charCountYear = charCount > 4 ? 4 : charCount;

                                if (!string.IsNullOrEmpty(parsedInfo.Year))
                                    numberChars = parsedInfo.Year.Right(charCountYear);
                                else
                                    numberChars = numberInfo.PubDate?.Year.ToString().Right(charCountYear);

                                builtNumber += numberChars;
                                break;
                            }

                        case 'G':
                            {
                                charCount = CountRepeatedChars(template, pos, 'G');
                                charCountYear = charCount > 4 ? 4 : charCount;

                                if (!string.IsNullOrEmpty(parsedInfo.Year))
                                    numberChars = parsedInfo.Year.Right(charCountYear);
                                else
                                    numberChars = numberInfo.IssRegDate?.Year.ToString().Right(charCountYear);

                                builtNumber += numberChars;
                                break;
                            }

                        case 'V':
                            {
                                charCount = 1;
                                builtNumber += numberChars + parsedInfo.City;
                                break;
                            }

                        case '(':
                            {
                                templateChars = template.Substring(pos + 1);
                                if (templateChars.IndexOf(")", StringComparison.Ordinal) >= 0)
                                {
                                    templateChars = templateChars.Substring(0, templateChars.IndexOf(")", StringComparison.Ordinal));

                                    charCount = templateChars.Length + 2;

                                    var charDateType = ' ';
                                    int? dateYear = null;

                                    if (templateChars.IndexOf("F", StringComparison.Ordinal) >= 0)
                                    {
                                        charDateType = 'F';
                                        dateYear = numberInfo.FilDate?.Year;
                                    }
                                    else if (templateChars.IndexOf("U", StringComparison.Ordinal) >= 0)
                                    {
                                        charDateType = 'U';
                                        dateYear = numberInfo.PubDate?.Year;
                                    }
                                    else if (templateChars.IndexOf("G", StringComparison.Ordinal) >= 0)
                                    {
                                        charDateType = 'G';
                                        dateYear = numberInfo.IssRegDate?.Year;
                                    }
                                    else if (templateChars.IndexOf("Y", StringComparison.Ordinal) >= 0)
                                    {
                                        charDateType = 'Y';
                                        dateYear = numberInfo.NumberDate?.Year;
                                    }

                                    if (char.IsLetter(charDateType))
                                    {
                                        char charOperator = ' ';

                                        var dateTypePos = templateChars.IndexOf(charDateType);

                                        if (templateChars.IndexOf("+", StringComparison.Ordinal) >= 0)
                                            charOperator = '+';
                                        else if (templateChars.IndexOf("-", StringComparison.Ordinal) >= 0)
                                            charOperator = '-';

                                        if (!char.IsWhiteSpace(charOperator))
                                        {
                                            charCountYear = CountRepeatedChars(templateChars, pos, charDateType);
                                            charCountYear = charCountYear > 4 ? 4 : charCountYear;

                                            switch (charOperator)
                                            {
                                                case '+':
                                                    {
                                                        numberChars = (dateYear ?? 0 + templateChars.GetConsecutiveDigits()).ToString().Right(charCountYear);
                                                        break;
                                                    }

                                                case '-':
                                                    {
                                                        numberChars = (dateYear ?? 0 - templateChars.GetConsecutiveDigits()).ToString().Right(charCountYear);
                                                        break;
                                                    }
                                            }

                                            builtNumber += numberChars;
                                        }
                                    }
                                }
                                else
                                    charCount = 1;
                                break;
                            }

                        default:
                            {
                                builtNumber = "";
                                break;

                            }
                    }

                    pos += charCount;
                }
            }

            return builtNumber;
        }




        private int CountRepeatedChars(string template, int startIndex, char charToFind)
        {
            var count = 0;
            for (var i = startIndex; i < template.Length; i++)
            {
                if (template[i] == charToFind)
                {
                    count++;
                }
                else break;
            }

            return count;
        }


        private int CountDigits(string number, int startIndex)
        {
            var count = 0;
            for (var i = startIndex; i < number.Length; i++)
            {
                if (int.TryParse(number[i].ToString(), out var n))
                {
                    count++;
                }
                else break;
            }
            return count;
        }



        private string GetCheckDigit(string number, WebLinksNumberInfoDTO numberInfo)
        {
            string factor;
            string totalStr;

            var checkDigit = "";
            var total = 0;
            var remainder = 0;

            // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            // B   R   A   Z   I   L
            // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            // *Brazil Law          i.e.    8  7  0  0  4  5  6
            // x 8  7  6  5  4  3  2
            // --------------------
            // 64 49  0  0  8 15 12
            // Total = (64 + 49 + 0 + 0 + 16 + 15 + 12) / 11 = 14R2
            // Check Digit = 11 - Remainder
            // = 11 - 2 = 9
            // If Remainder = 00 Then Check Digit is 0
            // If Remainder = 10 Then Check Digit is 0
            // 
            // '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            if (numberInfo.Country == "BR")
            {
                factor = numberInfo.NumberDate?.Year.ToString().Substring(2, 2) + number;

                if (factor.Length != 7)
                    checkDigit = "E";
                else
                {
                    for (var i = 1; i <= 7; i++)
                    {
                        total = Convert.ToInt32(factor.Substring(i, 1)) * (9 - i);
                    }
                    remainder = total % 11;
                    checkDigit = (11 - remainder).ToString();
                    if ((remainder == 0) || (remainder == 10) || (remainder == 1))
                        checkDigit = "0";
                }
            }

            //'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            //'  E   P   O
            //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            //' *EPO Law             i.e.               8  2  3  0  1  6  3  9
            //'                                       x 1  2  1  2  1  2  1  2
            //'                                        ------------------------
            //'                                         8  4  3  0  1 12  3 18
            //'       Total = (8 + 4 + 3 + 0 + 1 + 1 + 2 + 3 + 1 + 8) / 10
            //'       Check Digit = 10 - Remainder
            //                      '
            //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            //'       Format: concat strYear + strNumber
            //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            else if (numberInfo.Country == "EP")
            {
                factor = numberInfo.NumberDate?.Year.ToString().Substring(3, 2) + number;
                if (factor.Length != 8)
                    checkDigit = "E";
                else
                {
                    totalStr = "";
                    totalStr += factor.Substring(1, 1);
                    totalStr += (Convert.ToInt32(factor.Substring(2, 1)) * 2).ToString();
                    totalStr += factor.Substring(3, 1);
                    totalStr += (Convert.ToInt32(factor.Substring(4, 1)) * 2).ToString();
                    totalStr += factor.Substring(5, 1);
                    totalStr += (Convert.ToInt32(factor.Substring(6, 1)) * 2).ToString();
                    totalStr += factor.Substring(7, 1);
                    totalStr += (Convert.ToInt32(factor.Substring(8, 1)) * 2).ToString();

                    for (var i = 1; i <= totalStr.Length; i++)
                    {
                        total += Convert.ToInt32(totalStr.Substring(i, 1));
                    }

                    remainder = total % 10;
                    checkDigit = (10 - remainder).ToString();
                    if (checkDigit == "10")
                        checkDigit = "0";
                }
            }
            else if (numberInfo.Country == "GB")
            {
                factor = numberInfo.NumberDate?.Year.ToString().Substring(3, 2) + number;
                if (factor.Length != 7)
                    checkDigit = "E";
                else
                {
                    totalStr = "";
                    totalStr += (Convert.ToInt32(factor.Substring(1, 1)) * 2).ToString();
                    totalStr += factor.Substring(2, 1);
                    totalStr += (Convert.ToInt32(factor.Substring(3, 1)) * 2).ToString();
                    totalStr += factor.Substring(4, 1);
                    totalStr += (Convert.ToInt32(factor.Substring(5, 1)) * 2).ToString();
                    totalStr += factor.Substring(6, 1);
                    totalStr += (Convert.ToInt32(factor.Substring(7, 1)) * 2).ToString();

                    for (var i = 1; i <= totalStr.Length; i++)
                    {
                        total += Convert.ToInt32(totalStr.Substring(i, 1));
                    }

                    remainder = total % 10;
                    checkDigit = (10 - remainder).ToString();
                    if (checkDigit == "10")
                        checkDigit = "0";
                }
            }
            else if (numberInfo.Country == "DE")
            {
                if (numberInfo.CaseType != "UTM" && numberInfo.CaseType != "DIV")
                {
                    var deChkYear = numberInfo.FilDate?.Year;
                    if (deChkYear < 1981)
                    {
                        factor = number;
                        if (factor.Length != 7)
                            checkDigit = "E";
                        else
                        {
                            for (var i = 1; i <= 7; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (8 - i);
                            }
                            checkDigit = (total % 10).ToString();
                        }
                    }
                    else if (deChkYear >= 1981 && deChkYear <= 1994)
                    {
                        factor = number;
                        if (factor.Length != 7)
                            checkDigit = "E";
                        else
                        {

                            for (var i = 1; i <= 7; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (9 - i);
                            }
                            remainder = total % 11;
                            if (remainder == 0)
                                checkDigit = "0";
                            else if (remainder == 1)
                                checkDigit = "E";
                            else
                                checkDigit = (11 - remainder).ToString();
                        }
                    }
                    else if (deChkYear > 1994)
                    {
                        factor = "1" + number;
                        if (factor.Length != 8)
                            checkDigit = "E";
                        else
                        {
                            for (var i = 1; i <= 8; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (10 - i);
                            }
                            remainder = total % 11;
                            if (remainder == 0)
                                checkDigit = "0";
                            else if (remainder == 1)
                                checkDigit = "E";
                            else
                                checkDigit = (11 - remainder).ToString();
                        }
                    }
                }
                else if (numberInfo.CaseType == "UTM")
                {
                    var deChkYear = numberInfo.FilDate?.Year;
                    if (deChkYear < 1981)
                    {
                        factor = deChkYear.ToString().Substring(3, 2) + number;
                        if (factor.Length != 7)
                            checkDigit = "E";
                        else
                        {
                            for (var i = 1; i <= 7; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (8 - i);
                            }
                            remainder = total % 10;
                            checkDigit = remainder.ToString();
                        }
                    }
                    else if (deChkYear >= 1981 && deChkYear <= 1994)
                    {
                        factor = deChkYear.ToString().Substring(3, 2) + number;
                        if (factor.Length != 7)
                            checkDigit = "E";
                        else
                        {
                            for (var i = 1; i <= 7; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (9 - i);
                            }

                            remainder = total % 11;
                            if (remainder == 0)
                                checkDigit = "0";
                            else if (remainder == 1)
                                checkDigit = "E";
                            else
                                checkDigit = (11 - remainder).ToString();
                        }
                    }
                    else if (deChkYear > 1994)
                    {
                        factor = "2" + number;
                        if (factor.Length != 8)
                            checkDigit = "E";
                        else
                        {
                            for (var i = 1; i <= 8; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (10 - i);
                            }
                            remainder = total % 11;
                            if (remainder == 0)
                                checkDigit = "0";
                            else if (remainder == 1)
                                checkDigit = "E";
                            else
                                checkDigit = (11 - remainder).ToString();
                        }
                    }
                }
                else if (numberInfo.CaseType == "DIV")
                {
                    var deChkYear = numberInfo.FilDate?.Year;
                    if (deChkYear < 1981)
                    {
                        factor = deChkYear.ToString().Substring(3, 2) + number;
                        if (factor.Length != 7)
                            checkDigit = "E";
                        else
                        {
                            for (var i = 1; i <= 7; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (8 - i);
                            }
                            remainder = total % 10;
                            checkDigit = remainder.ToString();
                        }
                    }
                    else
                    {
                        factor = "1" + deChkYear.ToString().Substring(3, 2) + number;
                        if (factor.Length != 8)
                            checkDigit = "E";
                        else
                        {
                            for (var i = 1; i <= 8; i++)
                            {
                                total += Convert.ToInt32(factor.Substring(i, 1)) * (10 - i);
                            }
                            remainder = total % 11;
                            if (remainder == 0)
                                checkDigit = "0";
                            else if (remainder == 1)
                                checkDigit = "E";
                            else
                                checkDigit = (11 - remainder).ToString();
                        }
                    }
                }
            }

            return checkDigit;
        }

        private List<WebLinksNumberTemplateDTO> GetQualifiedTemplates(List<WebLinksNumberTemplateDTO> templates, string numberType, string number, DateTime? relativeDate)
        {
            string basedOn = string.Empty;

            if (numberType == WebLinksNumberType.AppNo)
            {
                basedOn = WebLinksDateType.Filing;
            }
            else if (numberType == WebLinksNumberType.PubNo)
            {
                basedOn = WebLinksDateType.Publication;
            }
            else if (numberType == WebLinksNumberType.PatRegNo)
            {
                basedOn = WebLinksDateType.IssueReg;
            }

            var validTemplates = templates.Where(t => IsTemplateValid(t, basedOn, number, relativeDate)).ToList();
            return validTemplates;
        }

        private bool IsTemplateValid(WebLinksNumberTemplateDTO template, string basedOn, string number, DateTime? relativeDate)
        {
            if (string.IsNullOrWhiteSpace(template.EffBasedOn) && template.MinLength == 0 && template.MaxLength == 0)
                return true;

            bool isValid = false;

            if (basedOn == template.EffBasedOn)
            {
                if (template.EffFromDate != null && template.EffToDate != null)
                {
                    if (relativeDate.HasValue && relativeDate >= template.EffFromDate && relativeDate <= template.EffToDate)
                        isValid = true;
                }
                else if (template.EffFromDate != null && template.EffToDate == null)
                {
                    if (relativeDate.HasValue && relativeDate >= template.EffFromDate)
                        isValid = true;
                }
                else if (template.EffFromDate == null && template.EffToDate != null)
                {
                    if (relativeDate.HasValue && relativeDate <= template.EffToDate)
                        isValid = true;
                }
                else if (template.EffFromDate == null && template.EffToDate == null)
                    isValid = true;
            }

            if ((isValid || (template.EffBasedOn == "")) && (template.MinLength > 0 || template.MaxLength > 0))
            {
                number = number.Trim();
                if (template.MinLength > 0 && template.MaxLength > 0)
                {
                    if (number.Length >= template.MinLength && number.Length <= template.MaxLength)
                        isValid = true;
                }
                else if (template.MinLength > 0)
                {
                    if (number.Length >= template.MinLength)
                        isValid = true;
                }
                else if (template.MaxLength > 0)
                {
                    if (number.Length <= template.MaxLength)
                        isValid = true;
                }
            }

            return isValid;
        }
    }

    public static class WebLinksNumberType
    {
        public const string AppNo = "A";
        public const string PubNo = "U";
        public const string PatRegNo = "P";
    }

    public static class WebLinksDateType
    {
        public const string Filing = "F";
        public const string Publication = "U";
        public const string IssueReg = "G";
    }

    public static class WebLinksTemplateType
    {
        public const string User = "S";
        public const string Web = "W";
        public const string Display = "D";
        public const string CPI = "C";
        public const string PL = "L";
    }

    public static class WebLinksSystemType
    {
        public const string Patent = "P";
        public const string Trademark = "T";
    }
}
