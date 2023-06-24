using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        private static class SyncEvents
        {
            public delegate void EventHandler_Void();
            public delegate void EventHandler_Int(int arg);
            public delegate void EventHandler_String(string arg);
            public delegate void EventHandler_StringBool(string arg1, bool arg2);


            public static event EventHandler_String AccountCreated;
            public static event EventHandler_String AccountCreated_RestoreSettings;
            public static event EventHandler_String AccountRemoved;
            public static event EventHandler_String AccountLoading;
            public static event EventHandler_String AccountLoaded;
            public static event EventHandler_StringBool AccountClosing;
            public static event EventHandler_StringBool AccountClosed;

            public static void CreateAccountAndRestoreSettings(string key) { AccountCreated?.Invoke(key); AccountCreated_RestoreSettings?.Invoke(key); }
            public static void RemoveAccount(string key) { AccountRemoved?.Invoke(key); }
            public static void LoadAccount(string key) { AccountLoading?.Invoke(key); AccountLoaded?.Invoke(key); }
            public static void CloseAccount(string key, bool loggedIn) { AccountClosing?.Invoke(key, loggedIn); AccountClosed?.Invoke(key, loggedIn); }


            public static event EventHandler_String LoggingIn;
            public static event EventHandler_Void LoginFailed;
            public static event EventHandler_String LoggedIn_Loading;
            public static event EventHandler_String LoggedIn_Loaded;
            public static event EventHandler_StringBool LoggedOut;

            public static void LogIn(string username) { LoggingIn?.Invoke(username); }
            public static void CancelLogin() { LoginFailed?.Invoke(); }
            public static void SucceedLogin(string username) { LoggedIn_Loading?.Invoke(username); LoggedIn_Loaded?.Invoke(username); }
            public static void LogOut(string username, bool auto) { LoggedOut?.Invoke(username, auto); }


            public static event EventHandler_Int PointsUpdated;

            public static void UpdatePoints(int points) { PointsUpdated?.Invoke(points); }


            public static event EventHandler_String OTPGetting;
            public static event EventHandler_String OTPGot;

            public static void GetOTP(string gameAccount) { OTPGetting?.Invoke(gameAccount); }
            public static void FinishGettingOTP(string otp) { OTPGot?.Invoke(otp); }


            public static event EventHandler_Void GameLaunching;
            public static event EventHandler_Void GameLaunched;

            public static void LaunchGame() { GameLaunching?.Invoke(); }
            public static void FinishLaunchingGame() { GameLaunched?.Invoke(); }
        }
    }
}
