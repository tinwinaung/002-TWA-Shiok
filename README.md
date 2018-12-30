## Shiok

Shiok is a borrowing from Malay that has acquired multiple uses and meanings in everyday Singapore English. It is an exclamation expressing admiration or approval, just like 'cool!' and 'great!'.

Shiok used opensource ClamAV Engine and Signature to scan specific folder in Web Application Servers (in realtime).

This software is free to use for any purpose.

## Why?

OpenSource virus scann engine are not available out of the box for realtime scanning in Windows. Here it is!

### Signature

Shiok (v0.5.6)

Release Date	: 31 Dec 2018

Author			  : TIN WIN AUNG

Copyright	  	: Copyright (C) 2018 TIN WINAUNG

Contact		  	: tinwinaung@sayargyi.com

# 1. Manual

## 1.1. Installation

Shiok install windows service call Shiok (Service Name: Shiok, Display Name: Shiok Antivirus).

Shiok include the following utilities (You can find in Windows Start Menu):

### 1.1.1. Shiok Controller
### 1.1.2. Shiok Service Configuration file
### 1.1.3. Folder Watcher list Configuration file
### 1.1.4. Uninstaler

## 1.2. Configuration

You can find configuration file in Start Menu (or) <Install Folder>\etc\ path.
The most important configuration is ShiokWatchFolders.txt where you can put all folders that you want to scan in realtiom. Please take note that you need to put full folder path (One path in a line each).
If you know well on ClamAV setting, you may play around with it under installed directory.

### 1.2.1. Shiok.txt 

It is used for advantage users. The most useful configurations will be proxy server for virus signature updates and email notifications.
You will also have option to scan folder everytime you start the service or disable it. Another important option is to allow virus scanning engine to use multi scan mode. If your processor is not powerful enough, it it better to disable it.

## 1.3. Uninstallation

You can just run uninstal program that you can find in start menu or installed folder. Please take note that we will not delete logs and configuration files. Uninstaller will open folder for you to delete manually if you want to.

# 2. Service

Shiok will start monitor the folder list when the service is started. However, full scanning of folder will wait until full virus signature database is download for the first time if you enable ScanFolderWhenStart option.
There will have a few seconds delay on full folder scan on Siok restart (If ScanFolderWhenStart is enabled) because Virus Engine to start first before scanning is started.

# 3. Virus Signature Database Update

Shiok allow you to disable build-in update process. In the case when you disable it, please make sure you have proper process to update database.

# 4. Log

I strongly advise you to audit Shiok.log file and make sure you read Fatal messages.
Currently, there is option to disable log options. (You need to know what is happening ... Right?)


