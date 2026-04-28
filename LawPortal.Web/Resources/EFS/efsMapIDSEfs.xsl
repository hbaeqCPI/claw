<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >
 
  <xsl:template match="/">
		<us-ids date-produced="20050902" dtd-version="v20_EFSWeb">
			<xsl:apply-templates select="NewDataSet/TableHeader" />
			<us-patent-cite>
				<xsl:apply-templates select="NewDataSet/TablePatentRef" />
			</us-patent-cite>
			<us-pub-appl-cite>
				<xsl:apply-templates select="NewDataSet/TablePublicationRef" />
			</us-pub-appl-cite>
			<us-foreign-document-cite>
				<xsl:apply-templates select="NewDataSet/TableForeignRef" />
			</us-foreign-document-cite>
			<xsl:apply-templates select="NewDataSet/TableNonPatentRef" />
      <us-ids-certification>
        <xsl:apply-templates select="NewDataSet/TableSignature" />
      </us-ids-certification>
    </us-ids>
	</xsl:template>

  <xsl:template match="TableHeader">
	<us-filing-info>
		<us-application-identification-info>
			<doc-number><xsl:value-of select="AppNumber"/></doc-number>
			<date><xsl:value-of select="FilDate"/></date>
		</us-application-identification-info>
		<us-first-named-inventor>
			<name><xsl:value-of select="FirstNamedInventor"/></name>
		</us-first-named-inventor>
		<file-reference-id><xsl:value-of select="AttorneyDocketNo"/></file-reference-id>
		<us-group-art-unit><xsl:value-of select="GroupArtUnit"/></us-group-art-unit>
		<primary-examiner>
			<name><xsl:value-of select="Examiner"/></name>
		</primary-examiner>
	</us-filing-info>
  </xsl:template>
    
  <xsl:template match="TablePatentRef">
	<us-doc-reference>
		<doc-number><xsl:value-of select="PatNumber"/></doc-number>
		<name><xsl:value-of select="RelatedFirstNamedInventor"/></name>
		<date><xsl:value-of select="IssDate"/></date>
    <kind><xsl:value-of select="KindCode"/></kind>
		<relevant-portion><xsl:value-of select="RefPages"/></relevant-portion>
	</us-doc-reference>
  </xsl:template>
  
  <xsl:template match="TablePublicationRef">
	<us-doc-reference>
		<doc-number><xsl:value-of select="PubNumber"/></doc-number>
		<name><xsl:value-of select="RelatedFirstNamedInventor"/></name>
		<date><xsl:value-of select="PubDate"/></date>
	  <kind>
	    <xsl:value-of select="KindCode"/>
	  </kind>
		<relevant-portion><xsl:value-of select="RefPages"/></relevant-portion>
	</us-doc-reference>
  </xsl:template>

  <xsl:template match="TableForeignRef">
	<us-foreign-doc-reference>
		<country><xsl:value-of select="Country"/></country>
		<doc-number><xsl:value-of select="PatNumber"/></doc-number>
		<name><xsl:value-of select="RelatedFirstNamedInventor"/></name>
		<date><xsl:value-of select="ForeignDate"/></date>
    <translation-attached>
      <xsl:value-of select="HasTranslation"/>
    </translation-attached>
	  <kind>
	    <xsl:value-of select="KindCode"/>
	  </kind>
		<relevant-portion><xsl:value-of select="RefPages"/></relevant-portion>
	</us-foreign-doc-reference>
  </xsl:template>
  
  <xsl:template match="TableNonPatentRef">
  	<us-nplcit>
		  <text><xsl:value-of select="NonPatLiteratureInfo"/></text>
	    <translation-attached>
	      <xsl:value-of select="HasTranslation"/>
	    </translation-attached>
	  </us-nplcit>
  </xsl:template>

  <xsl:template match="TableSignature">
    <applicant-name>
      <name>
        <xsl:value-of select="AttorneyName"/>
      </name>
      <registered-number>
        <name>
          <xsl:value-of select="RegistrationNumber"/>
        </name>
      </registered-number>
    </applicant-name>
    <electronic-signature>
      <date>
        <xsl:value-of select="CurrentDate"/>
      </date>
    </electronic-signature>
  </xsl:template>
  
</xsl:stylesheet>
