<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >

  <xsl:template match="/">
    <rcesb30>
      <xsl:apply-templates select="NewDataSet/TableCountryApp" />
      <Page2>
        <xsl:apply-templates select="NewDataSet/TableSignature" />
      </Page2>
    </rcesb30>
  </xsl:template>

  <xsl:template match="TableCountryApp">
    <filingdate>
      <xsl:value-of select="FilDate"/>
    </filingdate>
    <arttxt>
      <xsl:value-of select="GroupArtUnit"/>
    </arttxt>
    <examname>
      <xsl:value-of select="Examiner"/>
    </examname>
    <appno>
      <xsl:value-of select="AppNumber"/>
    </appno>
    <firstname>
      <xsl:value-of select="FirstNamedInventor"/>
    </firstname>
    <docnotxt>
      <xsl:value-of select="AttorneyDocketNo"/>
    </docnotxt>
  </xsl:template>

  <xsl:template match="TableSignature">
    <signpatent>
      <appsignform>
        <patentform>
          <appdatetxt>
            <xsl:value-of select="CurrentDate"/>
          </appdatetxt>
          <appsignnametxt>
            <xsl:value-of select="AttorneyName"/>
          </appsignnametxt>
          <pregnumbertxt>
            <xsl:value-of select="RegistrationNumber"/>
          </pregnumbertxt>
        </patentform>
      </appsignform>
    </signpatent>
  </xsl:template>


</xsl:stylesheet>
