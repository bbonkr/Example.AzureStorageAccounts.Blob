A snippet that gets a reference to a Blob entry in Azure Storage Accounts from a URI.

Strangely, every once in a while, the blob entry exists, but we can't find it, so we check it out.

## Preparation

Create an Azure Storage Accounts resource and enter the configuration, including the share key, into `appsettings.json`.

You need to enter AccountName, AccountKey, or ConnectionString.

## Run

Rename the `filelist.txt.sample` file in the project directory to `filelist.txt`.

Enter the URIs of the Blob entries you want to verify into the filelist.txt file.

Run the project.

## Verify

If the Blob item exists, despite multiple requests, you shouldn't see the item not found.
