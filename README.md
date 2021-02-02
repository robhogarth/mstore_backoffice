# mstore_backoffice

mstore backoffice repo contains C# solution and project for mstore.com.au code automation.

## Projects

### backoffice
This is the class library that contains classes that interact with Shopify and suppliers to do functions.  The core is the mbot class which contains all methods for interacting to the store front

### backoffice_test
This is a test class which is in it's infancy.  Over time more tests need t be added to ensure changes don't break the existing code base

### cloud_logging
Class for logging to the mstore_backoffice DB log system

### mstore_backoffice
.Net command line project which is the front end to running tasks in the mbot class.  This project is run on Jarvis to do different functions on a daily/hourly basis

### mstore_backoffice_gui
Beginning of a GUI interface to provide visual interface to the mbot.  No work has been done since v2 of the mbot was finished in Jan 2021, and is probably not compatible with any of the exisitng code base.
