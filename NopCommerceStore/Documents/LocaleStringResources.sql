﻿--new locale resources
declare @resources xml
set @resources='
<Language LanguageID="7">
  <LocaleResource Name="Admin.Categories.DisplayOrder">
    <Value>Display Order</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Categories.DisplayOrder">
    <Value>Display Order</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Categories.SaveButton.Text">
    <Value>Save</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Categories.SaveButton.ToolTip">
    <Value>Save changes</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Categories.DisplayOrder.RequiredErrorMessage">
    <Value>Display order is not specified</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Categories.DisplayOrder.RangeErrorMessage">
    <Value>Display order must be from -99999 to 99999</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Categories.ChangesSuccessfullySaved">
    <Value>Categories have been successfully updated</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.GlobalSettings.Other.HidNewsletterBox">
    <Value>Hide newsletter box:</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.GlobalSettings.Other.HidNewsletterBox.Tooltip">
    <Value>Check if you want to hide the newsletter subscription box</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Topics.IsPasswordProtected">
    <Value>Password protected</Value>
  </LocaleResource>
  <LocaleResource Name="TopicPage.btnPassword.Text">
    <Value>Enter</Value>
  </LocaleResource>
  <LocaleResource Name="TopicPage.WrongPassword">
    <Value>Wrong password</Value>
  </LocaleResource>
  <LocaleResource Name="TopicPage.EnterPassword">
    <Value>Please enter password to access this page:</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.TopicInfo.IsPasswordProtected">
    <Value>Is password protected:</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.TopicInfo.IsPasswordProtected.Tooltip">
    <Value>Check if this topic is password proceted</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.TopicInfo.Password">
    <Value>Password:</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.TopicInfo.Password.Tooltip">
    <Value>The password to access the content of this topic</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.TopicInfo.Url">
    <Value>URL:</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.TopicInfo.Url.Tooltip">
    <Value>The URL of this topic</Value>
  </LocaleResource>
</Language>
'

CREATE TABLE #LocaleStringResourceTmp
	(
		[LanguageID] [int] NOT NULL,
		[ResourceName] [nvarchar](200) NOT NULL,
		[ResourceValue] [nvarchar](max) NOT NULL
	)


INSERT INTO #LocaleStringResourceTmp (LanguageID, ResourceName, ResourceValue)
SELECT	@resources.value('(/Language/@LanguageID)[1]', 'int'), nref.value('@Name', 'nvarchar(200)'), nref.value('Value[1]', 'nvarchar(MAX)')
FROM	@resources.nodes('//Language/LocaleResource') AS R(nref)

DECLARE @LanguageID int
DECLARE @ResourceName nvarchar(200)
DECLARE @ResourceValue nvarchar(MAX)
DECLARE cur_localeresource CURSOR FOR
SELECT LanguageID, ResourceName, ResourceValue
FROM #LocaleStringResourceTmp
OPEN cur_localeresource
FETCH NEXT FROM cur_localeresource INTO @LanguageID, @ResourceName, @ResourceValue
WHILE @@FETCH_STATUS = 0
BEGIN
	IF (EXISTS (SELECT 1 FROM Nop_LocaleStringResource WHERE LanguageID=@LanguageID AND ResourceName=@ResourceName))
	BEGIN
		UPDATE [Nop_LocaleStringResource]
		SET [ResourceValue]=@ResourceValue
		WHERE LanguageID=@LanguageID AND ResourceName=@ResourceName
	END
	ELSE 
	BEGIN
		INSERT INTO [Nop_LocaleStringResource]
		(
			[LanguageID],
			[ResourceName],
			[ResourceValue]
		)
		VALUES
		(
			@LanguageID,
			@ResourceName,
			@ResourceValue
		)
	END
	
	IF (@ResourceValue is null or @ResourceValue = '')
	BEGIN
		DELETE [Nop_LocaleStringResource]
		WHERE LanguageID=@LanguageID AND ResourceName=@ResourceName
	END
	
	FETCH NEXT FROM cur_localeresource INTO @LanguageID, @ResourceName, @ResourceValue
	END
CLOSE cur_localeresource
DEALLOCATE cur_localeresource

DROP TABLE #LocaleStringResourceTmp
GO


