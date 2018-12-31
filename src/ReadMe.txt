
## Shiok

Meaning : Shiok is a borrowing from Malay that has acquired multiple uses and meanings in everyday Singapore English. It is an exclamation expressing admiration or approval, just like 'cool!' and 'great!'.

### Description

Shiok is a packag of windows service and related tools to scan specific folders and scan file in real-time by using open-source AntiVirus Engine and Virus Signature Database.

Shiok used Open-source [ClamAV](https://en.wikipedia.org/wiki/Clam_AntiVirus) AntiVirus Engine and Virus Signature Database. [ClamAV-Source](https://github.com/Cisco-Talos/clamav-devel)

### Features

1. Windows Service
2. Various configurations options for the Service
3. Multi threaded files/folders monitoring and scanning in real-time
-- If you are changing multiple files/folders in short time, threads will be managed
-- This can not be disabled
4. Multi scan will use all available CPU threads to scan files under folders.
-- This can be disabled in **etc\shiok.txt** file if you have less CPU cores
5. Email notification on Virus Found Events (With SMTP Server)
6. Proxy Server if you are behind firewall
7. Enable or Disable scan all file under watch folder when service is started
8. Drop-in* ClamAV update and configuration
--* You can just download and replace ClamAV directly into Program Foldera
-- You can configure additional ClamAV options in ***.conf.ref** files
8. Scheduler^ for Virus Signature Database update
-- ^ You can disable it so that you can do your own updates. 
-- However, Shiok have an implementation to wait for scanning while database is updating. 
-- You will not able to use this feature if you use your own scheduler.

#### Signature

Shiok (v0.5.6)
Release Date	: 1 Jan 2019
Author		: TIN WIN AUNG
Copyright	: Copyright (C) 2019 TIN WINAUNG
Contact		: tinwinaung@sayargyi.com

## Manual

### Installation

Shiok install windows service call Shiok (Service Name: Shiok, Display Name: Shiok Antivirus).
Shiok include the following utilities (You can find in Windows Start Menu):

1. Shiok Controller
2. Shiok Service Configuration file
3. Folder Watcher list Configuration file
4. Open Log Folder, Configuration Folder, and Virus Folder
5. Uninstaller

### Shiok Shell
You can use this command shell to 
1. Run Custom Scan
2. Run Manual Update
3. Control Service
-- Check Service Status
-- Add/Remove Service
-- Stop/Start Service

Please take note that this shell require elevated administrator privilege. Use Run As Administrator feature in Windows. Otherwise, the shall will request for it and re-launch itself.

### Configurations
#### etc\ShiokWatchFolders.txt

You can find configuration file in Start Menu (or) <Install Folder>\etc\ path.
The most important configuration is ShiokWatchFolders.txt where you can put all folders that you want to scan in real-time. Please take note that you need to put full folder path (One path in a line each).
If Shiok can not find the folder path, it will log under **etc\Shiok.log**

#### etc\Shiok.txt 

This file contain configurations for Shiok service. Details of each configuration option can be found inside the file.

### Uninstallation

You can just run Uninstall program that you can find in start menu or installed folder. Please take note that we will not delete logs and configuration files. Uninstaller will open folder for you to delete manually if you want to.

## Service

Shiok will start monitor and scan the folder list when the service is started. However, full scanning of folder will wait until full virus signature database is download for the first time if you enable **ScanFolderWhenStart** option in **Shiok.txt**. And also Shiok will update every time when the service is started.
There will also have a few seconds delay on full folder scan on Siok restart (If **ScanFolderWhenStart** is enabled) because Virus Engine will start first before scanning is started.

### Virus Signature Database Update

Shiok allow you to disable build-in update process. If you disable it, please make sure you have proper process to update database.

## Log

I strongly advise you to audit Shiok.log file and make sure you read Fatal messages.
Currently, there is option to disable log options. (You need to know what is happening ... Right?)

### ToDo
The followings will be updated. However, they are not directly related to Shiok Virus monitoring and Scanning.
1. Improved Shiok Shell
2. Add more configurations

Thanks

Tin
