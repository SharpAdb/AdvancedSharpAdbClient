#include "pch.h"
#include "winrt/AdvancedSharpAdbClient.WinRT.h"

using namespace winrt;
using namespace Windows::Foundation;
using namespace AdvancedSharpAdbClient::WinRT;

int main()
{
    init_apartment();
    auto adbServer = AdbServer::AdbServer();
    adbServer.StartServer(L"C:\\Program Files (x86)\\Android\\android-sdk\\platform-tools\\adb.exe", true);
    auto status = adbServer.GetStatus();
    printf("%ls\n", status.ToString().c_str());
    if (status.IsRunning())
    {
        auto adbClient = AdbClient::AdbClient();
        auto devices = adbClient.GetDevices();
        for (auto device : devices)
        {

        }
        adbClient.KillAdb();
    }
    system("pause");
}
