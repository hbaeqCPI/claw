<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >
 
  <xsl:template match="/">
	  <provisional>          
      <ContentArea1>            
					<xsl:apply-templates select="NewDataSet/TableInventors" />					  
      </ContentArea1>
      <ContentArea2>
        <xsl:apply-templates select="NewDataSet/TableApplicant" />
        <xsl:apply-templates select="NewDataSet/TableSignature" />
      </ContentArea2>  
        <xsl:apply-templates select="NewDataSet/TableCountryApp" />
    </provisional>
  </xsl:template>
     
  <xsl:template match="TableInventors">
    <sfApplicantInformation>
      <sfApplicantName>
        <firstName><xsl:value-of select="FirstName"/></firstName>
        <middleName><xsl:value-of select="MiddleInitial"/></middleName>
        <lastName><xsl:value-of select="LastName"/></lastName>
        <mailCountry><xsl:value-of select="Country"/></mailCountry>
        <city><xsl:value-of select="City"/></city>
        <state><xsl:value-of select="state"/></state>
	    </sfApplicantName>
    </sfApplicantInformation>
  </xsl:template>

  <xsl:template match="TableApplicant">
    <sfCorrCustNo>
      <customerNumber>
        <xsl:value-of select="CustomerNo"/>
      </customerNumber>      
    </sfCorrCustNo>
    <sfAppinfoFlow>
      <sfAppPos>
        <chkSmallEntity>
          <xsl:value-of select="TaxSchedule"/>
        </chkSmallEntity>
      </sfAppPos>
    </sfAppinfoFlow>   
  </xsl:template>

  <xsl:template match="TableSignature">
    <sfSig>
      <date>
        <xsl:value-of select="CurrentDate"/>
      </date>
      <first-name>
        <xsl:value-of select="AttorneyFirst"/>
      </first-name>
      <last-name>
        <xsl:value-of select="AttorneyLast"/>
      </last-name>
      <registration-number>
        <xsl:value-of select="RegistrationNumber"/>
      </registration-number>
    </sfSig>
  </xsl:template>
  
  <xsl:template match="TableCountryApp">
    <invention-title>
      <xsl:value-of select="AppTitle"/>
    </invention-title>
    <attorney-docket-number>
      <xsl:value-of select="AttorneyDocketNo"/>
    </attorney-docket-number>
  </xsl:template>


</xsl:stylesheet>
