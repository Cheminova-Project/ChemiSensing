# OAuth Unity Demo → Android device Setup Guide


# 1. Requirements

## Hardware

* Android tablet
* USB-C cable
* Windows PC or Mac with Unity installed

## Software

* Unity 2022 LTS or newer
* Android Build Support module installed
* Android SDK
* Android NDK
* OpenJDK

---

# 2. Verify Android Build Support

Open:

Unity Hub → Installs → Your Unity Version → Add Modules

Make sure these are installed:

* Android Build Support
* Android SDK & NDK Tools
* OpenJDK

If missing:

Install them before continuing.

---


# 3. Connect Android tablet to PC

Power on Android tablet.

Connect USB-C cable.

A popup appears:

"Allow USB Debugging?"

Check:

Always allow from this computer

Press:

Allow

---

# 4. Verify ADB Connection

Open terminal.

Run:

```bash
adb devices
```

Expected:

```text
List of devices attached
1WMHHXXXXXXXX device
```

If device does not appear:

* Reconnect cable
* Re-enable Developer Mode
* Accept USB debugging prompt again

---

# 5. Switch platform to Android

Open project.

Go to:

File → Build Settings

Select:

Android

Press:

Switch Platform

Wait until Unity finishes.

---

# 5. Configure Android Settings

Open:

Edit → Project Settings → Player Settings → Android

Set:

### Minimum API Level

```text
Android 10 (API 29)
```

### Target API Level

```text
Automatic
```

### Architecture

Enable:

```text
ARM64
```

Disable:

```text
ARMv7
```

---


## 6. Add Android Deep Link Support

Unity deep links on Android require an intent filter in `AndroidManifest.xml`.

### Enable a Custom Android Manifest

In Unity:

**Edit**
→ **Project Settings**
→ **Player**
→ **Android** tab
→ **Publishing Settings**
→ **Build**
→ **Custom Main Manifest**: **ON**

Unity will create:

```text
Assets/Plugins/Android/AndroidManifest.xml
```

Open that file.

---

### Add the Deep Link Intent Filter

Inside the main Unity activity, add:

```xml
<intent-filter>
    <action android:name="android.intent.action.VIEW" />

    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />

    <data
        android:scheme="chemianalysis"
        android:host="oauth"
        android:path="/callback" />
</intent-filter>
```

This tells Android:

```text
chemianalysis://oauth/callback
```

should open your Unity application.

---

### Where to Place It

If your manifest already contains:

```xml
<activity
    android:name="com.unity3d.player.UnityPlayerActivity"
    ...>
```

place the `<intent-filter>` **inside that activity element**, for example:

```xml
<activity
    android:name="com.unity3d.player.UnityPlayerActivity"
    ...>

    <intent-filter>
        <action android:name="android.intent.action.VIEW" />

        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />

        <data
            android:scheme="chemianalysis"
            android:host="oauth"
            android:path="/callback" />
    </intent-filter>

</activity>
```

Do **not** place the intent filter outside the activity.
---

# 7. Build APK

File → Build Settings

Select:

Android

Press:

Build

Save:

```text
OAuthDemoAndroid.apk
```

---

# 22. Deploy Directly to Headset

Tablet connected.

Press:

Build And Run
