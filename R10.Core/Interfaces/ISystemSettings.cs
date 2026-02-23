using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ISystemSettings<T> where T : new()
    {
        /// <summary>
        /// Get cached settings
        /// </summary>
        /// <returns></returns>
        Task<T> GetSetting();

        /// <summary>
        /// Refresh cached settings
        /// </summary>
        /// <returns></returns>
        Task Refresh();

        /// <summary>
        /// Get OptionValue as type T1 from table 
        /// using optionKey and optionSubKey parameters
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="optionKey"></param>
        /// <param name="optionName"></param>
        /// <returns></returns>
        Task<T1> GetValue<T1>(string optionKey, string optionSubKey);

        /// <summary>
        /// Get OptionValue as type T1 from table using 
        /// optionSubKey parameter and setting type as optionKey.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="optionName"></param>
        /// <returns></returns>
        Task<T1> GetValue<T1>(string optionSubKey);

        /// <summary>
        /// Get OptionValue as type T1 from table using 
        /// optionSubKey parameter and setting type as optionKey.
        /// Returns defaultValue parameter if optionName is not found or empty.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        Task<T1> GetValueOrDefault<T1>(string optionSubKey, T1 defaultValue);

        /// <summary>
        /// Get OptionValue as Array of type T1
        /// from table using optionKey and optionSubKey parameters.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="optionKey"></param>
        /// <param name="optionName"></param>
        /// <returns></returns>
        Task<T1[]> GetArrayValue<T1>(string optionKey, string optionSubKey);
    }
}
