﻿功能

> 支援 Beanfun (台灣)帳密登入與進階防護 (App 允許登入)或 QR Code 登入。    
> 取得新楓之谷登入帳號與一次性密碼。
> 
> **需要先安裝 [.NET 6 Desktop Runtime x64](https://aka.ms/dotnet/6.0/windowsdesktop-runtime-win-x64.exe)。**

與豬頭差異

1. 不支援楓之谷以外的遊戲
2. 重新設計的使用者介面
3. 有進階防護和剩餘點數
4. 沒有自動檢查更新和 Google Analytics

# MapleStoryLauncher

A program that launches and logs in TW MapleStory by Beanfun TW accounts.

Modified from BeanfunLogin:
> 
> Login to Beanfun and MapleStory without browsers, using 3DES to encrypt the stored password and DES to decrypt and get the OTP.
> 
> [Website](https://kevin940726.github.io/BeanfunLogin)

### License

This project is licensed under [GNU GPL v3 or later](COPYING), if not licensed separately.

## Changelog

v1.5.2
1. Updated the detection of requirement and completion of reCAPTCHA.

v1.5.1
1. Added reCAPTCHA to the procedure of logging in.
2. Updated and refactorred the protocol.
3. Cancelling connections in any way is much faster.
4. Updated the user-agent strings.
5. Fixes.

v1.5
1. Rewrote most UI-related codes and adjusted some UI behviours.
2. Slightly refined the protocol.

v1.4.2
1. Adjusted UI behaviours and fixed UI-related bugs.

v1.4.1
1. Updated the protocol to fix the auto-login.

v1.4
1. Now supports management of multiple accounts.
2. Improved stability.

v1.3.1
1. Now able to see remaining points.
2. Refined protocol.
3. Fixes.

v1.3
1. Rewrote network code and its interactions with the UI.
2. Now supports app authentication to log in.
3. Now possible to save the last response when unexpected errors have happened.
4. Adjusted UI appearance and behaviours.
5. Now warns of running multiple instances of the program.

v1.2 ~ v1.2.2
1. Adjusted UI appearance and behaviours and fixed UI-related bugs.

v1.1.1
1. Merged upstream and fixed a protocol issue.

v1.1
1. Brought back QRCode login method.
2. Adjusted UI behaviours and fixed UI-related bugs.
3. Minor fixes.

v1.0.1
1. Adjusted UI behaviours and fixed UI-related bugs.
2. Moved save file to %LocalAppData%\MaplestoryLauncher\.
3. Minor fixes.

v1.0
1. Reworked UI and its behaviours.
2. Removed functions(login methods now only traditional, stored accounts, game choices now only Maplestory, auto update checking).
3. Disabled Google Analytics to avoid contaminating the statistics.


<details>
<summary>Beanfun Login</summary>

Version 1.9.7
- Support KartRider/elsword

Version 1.9.5
- UI/UX optimization
- minor bug fixes
- recover password

Version 1.9.4
- Fix delay

Version 1.9.3
- Support QRCode login

Version 1.9.2
- Fix issue

Version 1.9.1
- Add QR Code login
- Add GA
- Remove unsupported login method
- Fix some bugs and error handling issues.

Version 1.8.1
- fix bugs:
  - auto select out of bound exception
  - check game path before open game

- features
  - user can choose not to open game autoly
  - keep logged in 
  
Version 1.7.0 
- multiple account and password save/load

Version 1.6.5
- Dynamic load game list.
- Add User-Agent at any request.

Version 1.6.3 
- Vakten update.

Version 1.6.2
- Support GAMAOTP (Thanks to 小艾).
- Auto select after login.

Version 1.6.1 
- Support other games of Beanfun.

Versiom 1.6.0 
- Support PlaySafe PKI API.
- Now can auto-login (responsed to the latest update of MapleStory).

Version 1.5.6 
- Testing PlaySafe PKI API.
- Added version log to the checking dialog.

Version 1.5.5 
- Fixed cannot get OTP bug.
- Disable keeplogged temporary.

Version 1.5.4 
- Added exception messages(Help me debug).
- Added some if-else statement in backgroundworker.
- Temporary disable the keeplogged feature.

Version 1.5.3
- Fixed OTPUnknown bug.
- Fixed OTP and GAMAOTP login bug.
- Now can scroll the account list.
- Disable the get OTP button when getting.
- Added some error messages.
- Still working on keeplogged bug.

Version 1.5.2 
- Fixed version check bug.

Version 1.5.1 
- Add version checking.

Version 1.5 
- Rewrite the code.

Version 1.4.4 
- Fixed ping bug (unstable).
- Added tooltips.

Version 1.4.3
- Changed ping url and fixed keeplogged bug.
- Fixed .NET textbox full mode bug.
- Fixed session key bug.

Version 1.4.2
- Remove auto generate OTP with double click event on listview.
- Fixed long delay session key bug.

Version 1.4.1
- Fixed re-login bug.

Version 1.4
- Now can login with PlaySafe (2nd generation only).

Version 1.3.4
- Fix the latest change of beanfun webtoken (f**k you!).

Version 1.3.3
- Bug fixed for re-login.
- Add feature to keep logged in.

Version 1.3.2
- Now can login with OTP (untested).
- Now can login with OTP type:E(聰明鎖) (untested).
- Fix login UI responsed to login method.

Version 1.3.1
- Now can login with GAMAOTP (untested).

Version 1.3
- Now can login with keypasco(金鑰一哥).
- Add login backgroundworker.
- Bug fixed.

Version 1.2.1
- Bug fixed.

Version 1.2 
- Adjust UI and UX.
- Seperate code.
- Fix bugs.
- Now can automatically run the game.
- Add background worker thread.

Version 1.1
- Adjust UI and UX.
- Replace icon with a big fat pig head (oh yeah).
- Fix bugs.
- Adjust code arrangement and add comments.

Version 1.0
- First deployment.
</details>
