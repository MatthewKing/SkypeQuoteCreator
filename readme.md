Skype Quote Creator
===================

A simple WinForms app that allows the user to create a new quote in Skype's clipboard format.

Get the application
-------------------

You can find the latest compiled version of this application at http://mking.s3.amazonaws.com/SkypeQuoteCreator.application

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