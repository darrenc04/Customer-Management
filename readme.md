# Customer Management App

A customer CRUD (add, edit, search, delete) made of three parts:

```
Mobile app (.NET MAUI)  -->  API (ASP.NET Core)  -->  SQL Server database
```

The app never connects to the database directly. It talks to the API, and only the API talks to the database.

**Always start things in this order: database, then API, then app.**

---

## What you need

| Item | Notes |
|---|---|
| Visual Studio with the **.NET Multi-platform App UI development** workload | To run the app |
| .NET SDK | Same version as the projects (check with `dotnet --version` in CMD) |
| SQL Server or **LocalDB** | LocalDB comes with Visual Studio |
| SSMS (SQL Server Management Studio) | To run the SQL scripts |
| Android emulator, an Android phone, or a Mac + iPhone | Only for the target you want to test |

---

## Step 1: Set up the database (once)

1. Open SSMS and connect to your server (LocalDB default: `(localdb)\MSSQLLocalDB`).
2. Run the SQL scripts in the `Database` folder **in numbered order**:
   create database, then tables, then stored procedures, then the user table and admin user.
3. Check it worked: `SELECT * FROM CustomerDB.dbo.AppUser;` should return one row (`admin`).

## Step 2: Configure the API (once)

Open `CustomerApi/appsettings.json`:

| Setting | What to do |
|---|---|
| `ConnectionStrings:DefaultConnection` | If your server is not `(localdb)\MSSQLLocalDB`, change the server name. In this JSON file every backslash is written twice: `(localdb)\\MSSQLLocalDB` |
| `Jwt:Key` | Replace the placeholder with your own long random text (32+ characters). Never share it. |

---

## Step 3: Start the API (CMD)

The API **must be running before you start the app**. It uses port **5108**
(set in the API's `Properties/launchSettings.json`, **http** profile).

1. Open **Command Prompt**.
2. Go to the folder that contains `CustomerApi.csproj`:
   ```
   cd <path to the folder containing CustomerApi.csproj>
   ```
3. Run **one** command, depending on where the app will run:

| App runs on | Command |
|---|---|
| Windows Machine | `dotnet run --launch-profile http` |
| Android emulator | `dotnet run --launch-profile http` |
| Real Android phone | `dotnet run --launch-profile http --urls "http://0.0.0.0:5108"` |
| iPhone | `dotnet run --launch-profile http --urls "http://0.0.0.0:5108"` |

`0.0.0.0` lets other devices on your Wi-Fi reach the API. Windows and the emulator don't need it.

4. Wait for the line `Now listening on: http://...:5108`.
   **Leave this window open.** Stop the API with **Ctrl+C**.

**Check the API is running:** open `http://localhost:5108/api/customers` in a browser.
- **401 Unauthorized (or a blank page)** = the API is running. This is the correct result, because no login token was sent.
- "This site can't be reached" = the API is not running, or the port is wrong.

---

## Step 4: Set the address the app uses (`Services/ApiSettings.cs`)

The app finds the API through this file. It already handles every device.
**Only `PcAddress` ever needs changing, and only for a real phone or iPhone.**

```csharp
using Microsoft.Maui.Devices;

namespace MauiApp1_Assignment1_.Services;

public static class ApiSettings
{
    private const int Port = 5108;

    // Real phone / iPhone only: your PC's IPv4 address (CMD: ipconfig)
    private const string PcAddress = "192.168.0.105";

    public static string BaseUrl
    {
        get
        {
            bool isEmulator = DeviceInfo.DeviceType == DeviceType.Virtual;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return isEmulator
                    ? $"http://10.0.2.2:{Port}/"        // Android emulator
                    : $"http://{PcAddress}:{Port}/";    // real Android phone
            }

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return isEmulator
                    ? $"http://localhost:{Port}/"       // iOS simulator (on a Mac)
                    : $"http://{PcAddress}:{Port}/";    // real iPhone
            }

            return $"http://localhost:{Port}/";          // Windows
        }
    }
}
```

| Device | Address the app uses | What you change |
|---|---|---|
| Windows Machine | `http://localhost:5108/` | Nothing |
| Android emulator | `http://10.0.2.2:5108/` | Nothing |
| Real Android phone | `http://<PcAddress>:5108/` | Set `PcAddress` |
| iPhone | `http://<PcAddress>:5108/` | Set `PcAddress` |

- **Find your PC address:** CMD, run `ipconfig`, use the **IPv4 Address** of your Wi-Fi adapter.
  `192.168.0.105` above is only an example. Use your own, and update it if your router gives the PC a new one.
- `Port` must match the API's port (5108). If you change one, change the other.
- After editing this file, **stop the app and run it again**. The address is read once at startup.

---

## Step 5: Run the app (Visual Studio)

Open the MAUI solution, choose the target in the dropdown next to the green Start button, and press **F5**.
The first build and install are slow.

### A. Windows Machine
1. Target: **Windows Machine**.
2. Turn on **Developer Mode**: Windows Settings > System > For developers.

### B. Android emulator
1. One-time: **Tools > Android > Android Device Manager** > New > Create, then **Start** it and wait for the home screen.
2. Target: the emulator (under **Android Emulators**).
3. `Platforms/Android/AndroidManifest.xml` must have `android:usesCleartextTraffic="true"` on the `<application>` tag (needed because the API uses plain `http`).
4. If the emulator won't start: turn on **Windows Hypervisor Platform** and **Virtual Machine Platform** (Windows Features), then restart the PC.

### C. Real Android phone
1. Phone: tap **Build number** 7 times, turn on **USB debugging**, plug in by USB, tap **Allow**.
2. PC and phone must be on the **same Wi-Fi**, and Windows must see that network as **Private**.
3. Open the firewall once. Run this **one line** in CMD **as administrator**:
   ```
   netsh advfirewall firewall add rule name="CustomerApi 5108" dir=in action=allow protocol=TCP localport=5108 profile=private
   ```
4. Set `PcAddress` in `ApiSettings.cs`, and start the API with the `0.0.0.0` command (Step 3).
5. **Test first:** in the phone's Chrome, open `http://<PcAddress>:5108/api/customers`. A **401** means it works.
   If the page doesn't load, the app can't work either, so fix this before continuing.
6. Target: your phone (under **Android Local Devices**).
7. The manifest needs `android:usesCleartextTraffic="true"` (same as B3).

### D. iPhone (needs a Mac)
iOS apps can only be built with a Mac. Windows alone cannot do it.
1. Mac: install **Xcode**, open it once, and turn on **Remote Login**.
2. Visual Studio: **Tools > iOS > Pair to Mac**.
3. iPhone: connect to the Mac by cable and turn on **Developer Mode** (Settings > Privacy and Security).
4. Set `PcAddress`, and start the API with the `0.0.0.0` command (Step 3).
5. Add this inside the main `<dict>` of `Platforms/iOS/Info.plist` (development only):
   ```xml
   <key>NSAppTransportSecurity</key>
   <dict><key>NSAllowsArbitraryLoads</key><true/></dict>
   <key>NSLocalNetworkUsageDescription</key>
   <string>The app connects to the customer API on your network.</string>
   ```
6. Run, tap **Allow** for local network access, and trust the developer app on the iPhone
   (Settings > General > VPN and Device Management).

Note: the iOS **simulator** uses `localhost`, which only reaches the API if the API runs on the same Mac. If the API runs on your Windows PC, use `PcAddress` for the simulator too.

---

## Login

| Username | Password |
|---|---|
| `admin` | `Admin@123` |

This demo account is created by the SQL script. Change the password before using the app for anything real.
The login lasts 60 minutes. After that, the app asks you to log in again.

---

## If something goes wrong

| Message / symptom | Fix |
|---|---|
| "Cannot reach the server" | API not running, wrong port, wrong `PcAddress`, firewall blocked, or the cleartext setting is missing |
| Works on the PC but not on the phone | The API was started without `--urls "http://0.0.0.0:5108"` |
| Phone's browser can't open the API address | Different Wi-Fi, the network is "Public", or the firewall rule is missing |
| Router blocks the phone (guest or company Wi-Fi) | Use a home network, or share a mobile hotspot from the phone and connect the PC to it |
| CMD error "A network-related or instance-specific error" | Wrong server name in the API's `appsettings.json` connection string |
| "Please enter the correct username and password" | Wrong login, or the user is missing from `dbo.AppUser` |
| "Your session has expired" | The token lasts 60 minutes; log in again |
| Postman returns 401 | Missing or expired Bearer token; log in again and paste the new token |
| Emulator is not in the target list | Start it first from Android Device Manager |
| Code changes have no effect | Stop and restart the API, and stop and re-run the app |

---

## Before publishing

- Use **https**, and remove the Android cleartext setting and the iOS `NSAllowsArbitraryLoads` setting.
- Replace `Jwt:Key` with your own secret, kept outside source control.
- Change the demo admin password.
