<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                exclude-result-prefixes="xsl wix">

  <xsl:output method="xml" indent="yes" omit-xml-declaration="no" />

  <!-- select name of directory as the project prefix -->
  <xsl:variable name="ProjectPrefix" select="//wix:Wix/wix:Fragment/wix:ComponentGroup/@Id" />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!--Give Component project prefix-->
  <xsl:template match="wix:Component/@Id|wix:ComponentRef/@Id|wix:File/@Id|wix:Directory/@Id">
    <xsl:attribute name="{name()}">
      <xsl:value-of select="concat($ProjectPrefix, '_', .)" />
    </xsl:attribute>
  </xsl:template>
</xsl:stylesheet>
