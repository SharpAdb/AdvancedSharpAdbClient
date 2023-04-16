#include "pch.h"
#include "winrt/AdvancedSharpAdbClient.WinRT.h"

using namespace winrt;
using namespace Windows::Foundation;
using namespace AdvancedSharpAdbClient::WinRT;

int main()
{
    init_apartment();
    auto a = AdbServer::AdbServer();
    a.StartServer(L"C:\\Program Files (x86)\\Android\\android-sdk\\platform-tools\\adb.exe", true);
    auto b = a.GetStatus();
    printf("%ls\n", b.ToString().c_str());
    system("pause");
}
