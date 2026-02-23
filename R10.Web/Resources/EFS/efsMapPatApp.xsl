<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >

	<xsl:template match="/">
		<us-request>
			<ContentArea1>
				<chkSecret>0</chkSecret>
				<xsl:apply-templates select="NewDataSet/TableInventors" />
			</ContentArea1>
			<ContentArea2>
				<xsl:apply-templates select="NewDataSet/TableApplicant" />
				<sfDomContinuityHeader />
				<xsl:apply-templates select="NewDataSet/TableDomesticBenefit" />
				<sfForeignPriorityHeader />
				<xsl:apply-templates select="NewDataSet/TableForeignPriority" />
				<xsl:apply-templates select="NewDataSet/TableSignature" />
			</ContentArea2>
			<ContentArea3/>
			<xsl:apply-templates select="NewDataSet/TableCountryApp" />
		</us-request>
	</xsl:template>

	<xsl:template match="TableInventors">
		<sfApplicantInformation>
			<sfAuth>
				<appSeq>
					<xsl:value-of select="AppSeq"/>
				</appSeq>
			</sfAuth>
			<sfApplicantName>
				<prefix />
				<firstName>
					<xsl:value-of select="FirstName"/>
				</firstName>
				<middleName>
					<xsl:value-of select="MiddleInitial"/>
				</middleName>
				<lastName>
					<xsl:value-of select="LastName"/>
				</lastName>
				<suffix />
			</sfApplicantName>
			<sfAppResChk>
				<resCheck>
					<ResidencyRadio>
						<xsl:value-of select="ResidencyFlag"/>
					</ResidencyRadio>
				</resCheck>
				<sfUSres>
					<rsCityTxt>
						<xsl:value-of select="USCity"/>
					</rsCityTxt>
					<rsStTxt>
						<xsl:value-of select="USState"/>
					</rsStTxt>
					<rsCtryTxt>
						<xsl:value-of select="USCountry"/>
					</rsCtryTxt>
				</sfUSres>
				<sfNonUSRes>
					<nonresCity>
						<xsl:value-of select="NonUSCity"/>
					</nonresCity>
					<nonresCtryList>
						<xsl:value-of select="NonUSCountry"/>
					</nonresCtryList>
				</sfNonUSRes>
				<sfMil>
					<actMilDropDown />
				</sfMil>
			</sfAppResChk>
			<sfCitz>
				<CitizedDropDown />
			</sfCitz>
			<sfApplicantMail>
				<mailCountry>
					<xsl:value-of select="Country"/>
				</mailCountry>
				<postcode>
					<xsl:value-of select="ZipCode"/>
				</postcode>
				<address1>
					<xsl:value-of select="Address1"/>
				</address1>
				<address2>
					<xsl:value-of select="Address2"/>
				</address2>
				<city>
					<xsl:value-of select="City"/>
				</city>
				<state>
					<xsl:value-of select="State"/>
				</state>
			</sfApplicantMail>
			<sfInventorRepInfo>
				<sfReporgChoice>
					<chkOrg />
					<org>
						<orgName />
					</org>
					<sfRepApplicantName>
						<prefix />
						<firstName />
						<middleName />
						<lastName />
						<suffix />
					</sfRepApplicantName>
				</sfReporgChoice>
				<sfRepAppResChk>
					<resCheck />
					<sfUSres>
						<rsCityTxt />
						<rsStTxt />
						<rsCtryTxt />
					</sfUSres>
					<sfNonUSRes>
						<nonresCity />
						<nonresCtryList />
					</sfNonUSRes>
					<sfMil>
						<actMilDropDown />
					</sfMil>
				</sfRepAppResChk>
				<sfRepCitz>
					<CitizedDropDown />
				</sfRepCitz>
				<sfRepApplicantMail>
					<address1 />
					<address2 />
					<city />
					<state />
					<postcode />
					<mailCountry />
				</sfRepApplicantMail>
			</sfInventorRepInfo>
		</sfApplicantInformation>
	</xsl:template>

	<xsl:template match="TableApplicant">
		<sfCorrepondInfo>
			<corresInfoChk>0</corresInfoChk>
		</sfCorrepondInfo>
		<sfCorrCustNo>
			<customerNumber>
				<xsl:value-of select="CustomerNo"/>
			</customerNumber>
		</sfCorrCustNo>
		<sfCorrAddress>
			<Name1 />
			<Name2 />
			<address1 />
			<address2 />
			<city />
			<state />
			<corrCountry />
			<postcode />
			<phone />
			<fax />
		</sfCorrAddress>
		<sfemail>
			<email />
		</sfemail>
		<sfInvTitle />
		<sfversion />
		<sfAppinfoFlow>
			<sfAppPos>
				<chkSmallEntity>
					<xsl:value-of select="TaxSchedule"/>
				</chkSmallEntity>
				<application_type>
					<xsl:value-of select="ApplicationType"/>
				</application_type>
				<us_submission_type>
					<xsl:value-of select="USSubmissionType"/>
				</us_submission_type>
			</sfAppPos>
		</sfAppinfoFlow>

		<sfPlant>
			<latin_name />
			<variety />
		</sfPlant>
		<sffilingby>
			<app />
			<date />
			<intellectual />
		</sffilingby>
		<sfPub>
			<early>0</early>
			<nonPublication />
		</sfPub>
		<sfRepHeader />
		<sfAttorny>
			<sfrepheader>
				<attornyChoice>customer-number</attornyChoice>
			</sfrepheader>
			<sfAttornyFlow>
				<sfcustomerNumber>
					<customerNumberTxt />
				</sfcustomerNumber>
				<sfAttrynyName>
					<prefix />
					<first-name />
					<middle-name />
					<last-name />
					<suffix />
					<attrnyRegNameTxt />
					<attsequence>1</attsequence>
				</sfAttrynyName>
				<sfrepcfr119>
					<repcfr119RegNameTxt />
					<prefix />
					<first-name />
					<middle-name />
					<last-name />
					<suffix />
					<repsequence>1</repsequence>
				</sfrepcfr119>
			</sfAttornyFlow>
		</sfAttorny>
	</xsl:template>


	<xsl:template match="TableDomesticBenefit">
		<sfDomesticContinuity>
			<sfDomesContinuity>
				<sfdomesContAppStat>
					<domAppStatusList>
						<xsl:value-of select="PriorStatus"/>
					</domAppStatusList>
					<domsequence>
						<xsl:value-of select="RowNo"/>
					</domsequence>
				</sfdomesContAppStat>
			</sfDomesContinuity>

			<xsl:if test="PriorStatus !='patented'">
				<sfDomesContInfo>
					<domappNumber>
						<xsl:value-of select="AppNo"/>
					</domappNumber>
					<domesContList>
						<xsl:value-of select="ContinuityType"/>
					</domesContList>
					<domPriorAppNum>
						<xsl:value-of select="PriorAppNo"/>
					</domPriorAppNum>
					<DateTimeField1>
						<xsl:value-of select="PriorFilDate"/>
					</DateTimeField1>
					<!--<domappNumber />
					<domesContList />
					<domPriorAppNum />
					<DateTimeField1 />-->					
				</sfDomesContInfo>
			</xsl:if>
			
			<sfDomesContinfoPatent>
				<patAppNum>
					<xsl:value-of select="AppNo"/>
				</patAppNum>
				<domesContList>
					<xsl:value-of select="ContinuityType"/>
				</domesContList>
				<patContType>
					<xsl:value-of select="PriorAppNo"/>
				</patContType>
				<patprDate>
					<xsl:value-of select="PriorFilDate"/>
				</patprDate>
				<patPatNum>
					<xsl:value-of select="PriorPatNo"/>
				</patPatNum>
				<patIsDate>
					<xsl:value-of select="PriorIssDate"/>
				</patIsDate>
			</sfDomesContinfoPatent>
		</sfDomesticContinuity>
	</xsl:template>
	
	<xsl:template match="TableForeignPriority">
		<sfForeignPriorityInfo>
			<frprAppNum>
				<xsl:value-of select="PrioNumber"/>
			</frprAppNum>
			<frprctryList>
				<xsl:value-of select="PrioCountry"/>
			</frprctryList>
			<frprParentDate>
				<xsl:value-of select="PrioDate"/>
			</frprParentDate>
			<accessCode>
				<xsl:value-of select="PrioAccessCode"/>
			</accessCode>
		</sfForeignPriorityInfo>
		<sfpermit>
			<check />
		</sfpermit>
		<AIATransition>
			<AIACheck>0</AIACheck>
		</AIATransition>
		<authorization>
			<IP>0</IP>
			<EPO>0</EPO>
		</authorization>
		<sfAssigneeHeader />
		<sfAssigneeInformation>
			<sfAssigneebtn>
				<appSeq>1</appSeq>
				<lstInvType />
				<LegalRadio />
			</sfAssigneebtn>
			<sfAssigneorgChoice>
				<chkOrg>0</chkOrg>
				<sforgName>
					<orgName />
				</sforgName>
			</sfAssigneorgChoice>
			<sfApplicantName>
				<prefix />
				<first-name />
				<middle-name />
				<last-name />
				<suffix />
			</sfApplicantName>
			<sfAssigneeAddress>
				<address-1 />
				<address-2 />
				<city />
				<state />
				<postcode />
				<phone />
				<fax />
				<txtCorrCtry />
			</sfAssigneeAddress>
			<sfAssigneeEmail />
		</sfAssigneeInformation>
		<NonApplicantHeader />
		<sfNonApplicantInfo>
			<sfNonAsigneeBtn>
				<appSeq>1</appSeq>
			</sfNonAsigneeBtn>
			<sfNonapplicantOrg>
				<chkOrg>0</chkOrg>
				<sfNonOrg>
					<orgName />
				</sfNonOrg>
			</sfNonapplicantOrg>
			<sfApplicantName>
				<prefix />
				<first-name />
				<middle-name />
				<last-name />
				<suffix />
			</sfApplicantName>
			<sfAssigneeAddress>
				<address-1 />
				<address-2 />
				<city />
				<state />
				<postcode />
				<phone />
				<fax />
				<txtCorrCtry />
				<email />
			</sfAssigneeAddress>
		</sfNonApplicantInfo>
	</xsl:template>

	<xsl:template match="TableSignature">
		<sfSignature>
			<sfSigHeader/>
			<sfSig>
				<registration-number>
					<xsl:value-of select="RegistrationNumber"/>
				</registration-number>
				<date>
					<xsl:value-of select="CurrentDate"/>
				</date>
				<first-name>
					<xsl:value-of select="AttorneyFirst"/>
				</first-name>
				<last-name>
					<xsl:value-of select="AttorneyLast"/>
				</last-name>
				<signature>
					<xsl:value-of select="AttorneySignature"/>
				</signature>
			</sfSig>
		</sfSignature>
	</xsl:template>

	<xsl:template match="TableCountryApp">
		<invention-title>
			<xsl:value-of select="AppTitle"/>
		</invention-title>
		<attorney-docket-number>
			<xsl:value-of select="AttorneyDocketNo"/>
		</attorney-docket-number>
		<version-info>2.1</version-info>
		<clientversion>22.00320282</clientversion>
		<!--<numofpages>8</numofpages>-->
		<!-- first page appnumber -->
		<application-number>
			<xsl:value-of select="AppNumber"/>
		</application-number>

		<!-- second page appnumber -->
		<sfHeader>
			<application-number>
				<xsl:value-of select="AppNumber"/>
			</application-number>
		</sfHeader>

		<!-- looking for 3rd page appnumber -->
		<!-- below not working!!!!! for 3rd page appnumber – seems that there is no actual field on this spot
    <sfHeader>
      <xsl:value-of select="AppNumber"/>
    </sfHeader>

    <sfADSHeaderInfo>
      <sfHeader>
        <application-number>
          <xsl:value-of select="AppNumber"/>
        </application-number>
      </sfHeader>
    </sfADSHeaderInfo>

    <sfADSHeaderInfo>
      <sfHeader>
        <xsl:value-of select="AppNumber"/>
      </sfHeader>
    </sfADSHeaderInfo>

    <sfADSHeaderInfo>
      <xsl:value-of select="AppNumber"/>
    </sfADSHeaderInfo>
    
    <sfADSHeaderInfo>
      <application-number>
        <xsl:value-of select="AppNumber"/>
      </application-number>
    </sfADSHeaderInfo>
    
    -->
	</xsl:template>
</xsl:stylesheet>
