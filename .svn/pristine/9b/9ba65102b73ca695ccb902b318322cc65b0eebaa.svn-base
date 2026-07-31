# ChemiNova OAuth Demo – Unity to iOS Deployment Guide

This document describes the minimum steps required to build and deploy the Unity OAuth demo application to an iPad.

---

## Prerequisites

Required on the build Mac:

- Unity installed
- Unity iOS Build Support module installed
- Xcode installed
- Access to an Apple Developer Team
- Physical iPad available for testing

---

## 1. Open the Unity Project

Open the project in Unity.

---

## 2. Switch Platform to iOS

Navigate to:

File → Build Settings → iOS → Switch Platform

Wait for Unity to re-import assets.

---

## 3. Configure Bundle Identifier

Navigate to:

Edit → Project Settings → Player → iOS

Under **Identification**, set:

- Company Name
- Product Name
- Bundle Identifier (must be unique)

---

## 4. Configure Deep Link URL Scheme

Navigate to:

Edit → Project Settings → Player → iOS

Go to:

Other Settings → Configuration → Supported URL Schemes

Add:

```
chemianalysis
```

---

## 5. Build the Xcode Project

Navigate to:

File → Build Settings → iOS → Build

Choose an output folder.

Unity will generate an Xcode project.

---

## 6. Open the Project in Xcode

Open:

Builds/iOS/Unity-iPhone.xcodeproj

In Xcode:

- Select **Unity-iPhone** (blue project icon)
- Go to **TARGETS → Unity-iPhone**
- Open **Signing & Capabilities**
- Enable **Automatically manage signing**
- Select your **Team** (Apple Developer Team)

---

## 7. Connect the iPad

Connect the iPad to the Mac.

If prompted:

- Tap **Trust This Computer**
- Enter the device passcode

---

## 8. Enable Developer Mode on the iPad

On the iPad:

Settings → Privacy & Security → Developer Mode → Enable

The device will reboot.

After reboot, confirm Developer Mode again.

---

## 9. Build and Run

In Xcode:

- Select the connected iPad from the device dropdown
- Press ▶ Run