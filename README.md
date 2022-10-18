# AppleStockChecker
**License: MIT**

**.NET 6.0 or newer required**

## What is this?
* This is an app I created to track in-store availability of Apple products on a minute-per-minute basis.
* It is fully configurable to allow you to specify which products, the model, the area you want to check store availability for, etc.
* With a modular notification system, you can get notifications however you like! Read on for more information.

## Types of notifications
1. **Built-in toast notifications:** Whenever something you're interested in becomes available, a toast notification will appear on your desktop showing you what it is and where it is.
2. **Text (SMS) notifications:** Included in this repository is a "notify module" called TwilioNotify.
This is an external library that can be loaded by the stock checker. It is an external module that provides mobile text notifications; just in case you're super desperate.

Notify modules are tiny libraries you can write to customize your notifications, and to get through any medium you want!
All you need to do is implement the the stock checker's `INotifyInterface`. See TwilioNotify for an example of this.

## Running
After compiling the project, you can use `dotnet run` in the executable directory to run the stock checker.
Note that any notify modules you want to use must be in the same directory. By default, TwilioNotify is built in the stock checker's 'bin' directory already.
On the first run, you will be asked to complete the configuration. An example configuration can be found below.

## Configuring
A `config.json` file must be in the same directory as the executable. A blank `config.json` will be created on the first run, if it doesn't exist already.

Here is an example `config.json` that checks stock for iPhone 14 models:
```
{
    "headers": {
        "user-agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36",
        "cache-control": "no-cache",
        "pragma": "no-cache"
    },
    "models": [
        {
          "name": "128GB Deep Purple Pro",
          "part": "MQ0E3LL/A",
          "active": true
        },
        {
          "name": "128GB Space Black Pro",
          "part": "MPXT3LL/A",
          "active": false
        },
    ],
    "profiles": [
        {
            "name": "NYC",
            "postalCode": 10001,
            "checkMins": 2,
            "suppressInStockMins": 10,
            "active": true
        }
    ],
    "queryTimeoutMs": 60000,
    "fixedParams": {
        "pl": "true",
        "cppart": "UNLOCKED/US"
    },
    "notifyModules": [
        "TwilioNotify"
    ],
    "notifyOnError": false
}
```
