using System.Collections.Generic;

namespace CommonUtil.Android
{
    public static class AndroidUtil
    {
        private static string androidHome = null;
        private static int hasAndroidSdk = -1;
        public static bool HasAndroidSdk
        {
            get
            {
                if (hasAndroidSdk == -1)
                {
                    // TODO: Mac/Linux
                    // TODO: do a fallback check to %appdata%\Android\Sdk
                    string androidSdk = System.Environment.GetEnvironmentVariable("ANDROID_HOME");
                    hasAndroidSdk = 0;
                    if (androidSdk != null && androidSdk.Length != 0)
                    {
                        if (System.IO.File.Exists(System.IO.Path.Combine(androidSdk, "platform-tools", "adb.exe")))
                        {
                            hasAndroidSdk = 1;
                            androidHome = androidSdk;
                        }
                    }
                }
                return hasAndroidSdk == 1;
            }
        }

        public static AndroidApkBuildResult BuildApk(string androidProjectDirectory)
        {
            GradleProcess p = new GradleProcess(androidProjectDirectory);
            p.RunBlocking();
            string output = p.Output;
            string[] lines = output.Trim().Split('\n');
            string secondToLast = lines.Length > 2 ? lines[lines.Length - 2] : ""; // Super robust code right here!
            if (secondToLast.Trim().StartsWith("BUILD SUCCESSFUL"))
            {
                return new AndroidApkBuildResult() { HasError = false, ApkPath = System.IO.Path.Combine(androidProjectDirectory, "app", "build", "outputs", "apk", "debug", "app-debug.apk") };
            }
            return new AndroidApkBuildResult() { HasError = true, Error = output };
        }

        private class GradleProcess : Processes.Process
        {
            private List<string> output = new List<string>();
            public string Output { get { return string.Join('\n', output); } }

            internal GradleProcess(string projDir)
                : base(System.IO.Path.Combine(projDir, "gradlew.bat"), "assembleDebug")
            {
                this.SetWorkingDirectory(projDir);
            }

            public override void OnStdErrReceived(string data)
            {
                this.output.Add(data);
            }

            public override void OnStdOutReceived(string data)
            {
                this.output.Add(data);
            }
        }
    }
}
