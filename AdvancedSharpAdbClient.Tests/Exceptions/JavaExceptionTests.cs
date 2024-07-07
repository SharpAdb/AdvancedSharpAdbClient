using System;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class JavaExceptionTests
    {
        [Fact]
        public void EmptyConstructorTest() =>
            ExceptionTester<JavaException>.EmptyConstructorTest(() => new JavaException(string.Empty, string.Empty));

        [Fact]
        public void MessageConstructorTest() =>
            ExceptionTester<JavaException>.MessageConstructorTest(message => new JavaException(string.Empty, message, string.Empty));

        [Fact]
        public void MessageAndInnerConstructorTest() =>
            ExceptionTester<JavaException>.MessageAndInnerConstructorTest((message, inner) => new JavaException(string.Empty, message, string.Empty, inner));

        [Fact]
        [Obsolete]
        public void SerializationConstructorTest() =>
            ExceptionTester<JavaException>.SerializationConstructorTest((info, context) => new JavaException(info, context));

        [Fact]
        public void ParseLineTest()
        {
            string line =
                """
                java.lang.SecurityException: Caller has no access to session 936013062
                        at com.android.server.pm.PackageInstallerService.openSessionInternal(PackageInstallerService.java:849)
                        at com.android.server.pm.PackageInstallerService.openSession(PackageInstallerService.java:839)
                        at com.android.server.pm.PackageManagerShellCommand.doWriteSplit(PackageManagerShellCommand.java:3270)
                        at com.android.server.pm.PackageManagerShellCommand.runInstallWrite(PackageManagerShellCommand.java:1450)
                        at com.android.server.pm.PackageManagerShellCommand.onCommand(PackageManagerShellCommand.java:202)
                        at android.os.BasicShellCommandHandler.exec(BasicShellCommandHandler.java:98)
                        at android.os.ShellCommand.exec(ShellCommand.java:44)
                        at com.android.server.pm.PackageManagerService.onShellCommand(PackageManagerService.java:22344)
                        at android.os.Binder.shellCommand(Binder.java:940)
                        at android.os.Binder.onTransact(Binder.java:824)
                        at android.content.pm.IPackageManager$Stub.onTransact(IPackageManager.java:4644)
                        at com.android.server.pm.PackageManagerService.onTransact(PackageManagerService.java:4513)
                        at android.os.Binder.execTransactInternal(Binder.java:1170)
                        at android.os.Binder.execTransact(Binder.java:1134)
                """;

            JavaException javaException = JavaException.Parse(line);

            Assert.Equal("SecurityException", javaException.JavaName);
            Assert.Equal("Caller has no access to session 936013062", javaException.Message);
            Assert.Equal(
                """
                        at com.android.server.pm.PackageInstallerService.openSessionInternal(PackageInstallerService.java:849)
                        at com.android.server.pm.PackageInstallerService.openSession(PackageInstallerService.java:839)
                        at com.android.server.pm.PackageManagerShellCommand.doWriteSplit(PackageManagerShellCommand.java:3270)
                        at com.android.server.pm.PackageManagerShellCommand.runInstallWrite(PackageManagerShellCommand.java:1450)
                        at com.android.server.pm.PackageManagerShellCommand.onCommand(PackageManagerShellCommand.java:202)
                        at android.os.BasicShellCommandHandler.exec(BasicShellCommandHandler.java:98)
                        at android.os.ShellCommand.exec(ShellCommand.java:44)
                        at com.android.server.pm.PackageManagerService.onShellCommand(PackageManagerService.java:22344)
                        at android.os.Binder.shellCommand(Binder.java:940)
                        at android.os.Binder.onTransact(Binder.java:824)
                        at android.content.pm.IPackageManager$Stub.onTransact(IPackageManager.java:4644)
                        at com.android.server.pm.PackageManagerService.onTransact(PackageManagerService.java:4513)
                        at android.os.Binder.execTransactInternal(Binder.java:1170)
                        at android.os.Binder.execTransact(Binder.java:1134)
                """, javaException.JavaStackTrace, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ParseLinesTest()
        {
            string[] lines =
                """
                java.lang.SecurityException: Caller has no access to session 936013062
                        at com.android.server.pm.PackageInstallerService.openSessionInternal(PackageInstallerService.java:849)
                        at com.android.server.pm.PackageInstallerService.openSession(PackageInstallerService.java:839)
                        at com.android.server.pm.PackageManagerShellCommand.doWriteSplit(PackageManagerShellCommand.java:3270)
                        at com.android.server.pm.PackageManagerShellCommand.runInstallWrite(PackageManagerShellCommand.java:1450)
                        at com.android.server.pm.PackageManagerShellCommand.onCommand(PackageManagerShellCommand.java:202)
                        at android.os.BasicShellCommandHandler.exec(BasicShellCommandHandler.java:98)
                        at android.os.ShellCommand.exec(ShellCommand.java:44)
                        at com.android.server.pm.PackageManagerService.onShellCommand(PackageManagerService.java:22344)
                        at android.os.Binder.shellCommand(Binder.java:940)
                        at android.os.Binder.onTransact(Binder.java:824)
                        at android.content.pm.IPackageManager$Stub.onTransact(IPackageManager.java:4644)
                        at com.android.server.pm.PackageManagerService.onTransact(PackageManagerService.java:4513)
                        at android.os.Binder.execTransactInternal(Binder.java:1170)
                        at android.os.Binder.execTransact(Binder.java:1134)
                """.Split(Extensions.NewLineSeparator);

            JavaException javaException = JavaException.Parse(lines);

            Assert.Equal("SecurityException", javaException.JavaName);
            Assert.Equal("Caller has no access to session 936013062", javaException.Message);
            Assert.Equal(
                """
                        at com.android.server.pm.PackageInstallerService.openSessionInternal(PackageInstallerService.java:849)
                        at com.android.server.pm.PackageInstallerService.openSession(PackageInstallerService.java:839)
                        at com.android.server.pm.PackageManagerShellCommand.doWriteSplit(PackageManagerShellCommand.java:3270)
                        at com.android.server.pm.PackageManagerShellCommand.runInstallWrite(PackageManagerShellCommand.java:1450)
                        at com.android.server.pm.PackageManagerShellCommand.onCommand(PackageManagerShellCommand.java:202)
                        at android.os.BasicShellCommandHandler.exec(BasicShellCommandHandler.java:98)
                        at android.os.ShellCommand.exec(ShellCommand.java:44)
                        at com.android.server.pm.PackageManagerService.onShellCommand(PackageManagerService.java:22344)
                        at android.os.Binder.shellCommand(Binder.java:940)
                        at android.os.Binder.onTransact(Binder.java:824)
                        at android.content.pm.IPackageManager$Stub.onTransact(IPackageManager.java:4644)
                        at com.android.server.pm.PackageManagerService.onTransact(PackageManagerService.java:4513)
                        at android.os.Binder.execTransactInternal(Binder.java:1170)
                        at android.os.Binder.execTransact(Binder.java:1134)
                """, javaException.JavaStackTrace, ignoreLineEndingDifferences: true);
        }
    }
}
