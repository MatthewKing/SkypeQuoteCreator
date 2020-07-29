Skype Quote Creator
===================

A simple WinForms app that allows the user to create a new quote in Skype's clipboard format.

Deprecation notice
------------------

Please note that this app is no longer actively maintained, and won't work with newer versions of Skype. Skype changed their quote system years ago, and this app does not support their new quote system. I'm no longer an active Skype user so I don't know the details of how the new quote system works. I'd be happy to accept a pull request from any community members who managed to work it out, though.

Get the application
-------------------

You can download the application from the following sources:

| Application                                                                 | Description                                |
|-----------------------------------------------------------------------------|--------------------------------------------|
| [Recommended](https://mking.s3.amazonaws.com/SkypeQuoteCreator.application) | ClickOnce installer with automatic updates |
| [Stand-alone](https://mking.s3.amazonaws.com/SkypeQuoteCreator.exe)         | Stand-alone executable                     |

Skype clipboard format
----------------------

When a Skype quote is copied to the clipboard, the data object looks something like this:

| Format               | Description                                                     |
|----------------------|-----------------------------------------------------------------|
| System.String        | Plain-text message                                              |
| Text                 | Plain-text message                                              |
| UnicodeText          | Plain-text message                                              |
| OEMText              | Plain-text message                                              |
| SkypeMessageFragment | MemoryStream containing UTF8 Skype message fragment (see below) |
| Locale               | MemoryStream containing Cultural identifier (LCID)              |

SkypeMessageFragment
------------------

A simple XML string in the following format: `<quote author="AuthorName" timestamp="Timestamp">MessageText</quote>`, where `AuthorName` is the Skype user name of the message author, `MessageText` is the message text itself, and `Timestamp` is the number of seconds since the Unix epoch. The actual fragment from Skype contains a few more attributes, but they can be ignored for the purposes of this simple app!

Copyright
---------
Copyright 2013-2016 Matthew King.

License
-------
Skype Quote Creator is licensed under the [MIT License](http://opensource.org/licenses/MIT). Refer to license.txt for more information.
