# 🛒 ECommerce-Api - Simple REST API for Online Stores

[![Download Latest Release](https://img.shields.io/badge/Download-ECommerce--Api-blue?style=for-the-badge)](https://github.com/LucaRain/ECommerce-Api/releases)

## 🚀 What is ECommerce-Api?

ECommerce-Api is a ready-to-use backend service for online stores. It helps manage products, customers, orders, and payments. The API follows clear rules to handle data safely and quickly. It runs on Windows and uses fast databases to keep everything smooth. This product is designed for everyday use, not just testing or learning.

You do not need to understand programming to use it. This guide will walk you through downloading and running the software on your Windows PC.

---

## 🔍 Key Features

- Manage product listings with detailed information
- Handle user registration and secure login
- Process orders, track status, and handle payments
- Use fast PostgreSQL database to store data
- Use Redis cache to speed up responses
- Secure access with token-based login
- Support for clean, organized backend structure

---

## 💻 System Requirements

- Windows 10 or later (64-bit)
- At least 4 GB of RAM
- Minimum 2 GHz dual-core processor
- Free disk space: 1 GB for installation and data
- Active internet connection for initial setup
- No programming tools required

---

## 🛠️ Before You Start

Make sure your PC meets the system requirements above. You need a stable internet connection to download the files and a simple tool called Command Prompt (built into Windows) for running commands.

---

## 📥 How to Download and Install ECommerce-Api

Click the badge below or visit the link to download the software.

[![Download Latest Release](https://img.shields.io/badge/Download-ECommerce--Api-blue?style=for-the-badge)](https://github.com/LucaRain/ECommerce-Api/releases)

1. Visit the link: https://github.com/LucaRain/ECommerce-Api/releases  
2. Look for the latest release. It will have a name like `ECommerce-Api-v1.0.zip` or similar.  
3. Click on the zip file to download it to your PC.  
4. Once downloaded, open the folder where the file is saved.  
5. Right-click the zip file and choose **Extract All...**  
6. Select a folder to extract the files and click **Extract**.  
7. Open the extracted folder to see the program files.

---

## ⚙️ How to Run the Application

ECommerce-Api runs as a background service on your PC. You will use simple commands to start it.

1. Open **Command Prompt**:
    - Press the Windows key.
    - Type `cmd`.
    - Press Enter.

2. Use the `cd` command to go to the folder where you extracted ECommerce-Api. For example:  
   ```
   cd C:\Users\YourName\Downloads\ECommerce-Api
   ```

3. Start the application by running the main executable file. You may see a file named something like `ECommerce.Api.exe` or instructions file named `README-SETUP.txt` in the folder that explains the exact command. Usually, this looks like:  
   ```
   ECommerce.Api.exe
   ```

4. The program will start the backend API server on your PC. You should see messages indicating it is running.

---

## 🔌 What Happens Next? Using the API

Once started, ECommerce-Api runs on your PC as a server. You or any front-end application can connect to it using simple web addresses called URLs. For example:  
```
http://localhost:5000/api/products
```

This address sends a request to get product information. You do not need to understand this if you only want to run the software. But this shows you the software is ready to handle real e-commerce tasks.

---

## 🗃️ Managing Data with PostgreSQL and Redis

ECommerce-Api uses two main systems to work fast:

- **PostgreSQL:** This is where all the data like products, users, and orders are saved.  
- **Redis:** This works as a short-term memory to speed up common requests.

Both systems are included in the download, and ECommerce-Api handles connecting to them automatically. You do not need to set them up yourself.

---

## 🔐 Security and Access

This software uses secure login technology. It creates timed tokens that let users safely access their accounts without sharing passwords every time.

You do not have to do anything for this during install. The system activates as soon as you start the application.

---

## ⚠️ Common Troubleshooting

- If you cannot find the executable to start the API, check the extracted folder again. Look for `.exe` files or `.bat` files.  
- If the Command Prompt stops, try reopening it and checking your folder path carefully.  
- Make sure no other program is using port 5000 or the port mentioned in the start messages.  
- Restart your PC and try again if responses are slow or missing.  
- Check your internet connection during the download phase.

---

## 📂 Useful Links and Resources

- Download page: https://github.com/LucaRain/ECommerce-Api/releases  
- ECommerce-Api topics: asp-net, clean-architecture, docker, docker-compose, e-commerce, entity-framework-core, jwt-auth, openapi, postgresql, redis, scalar

You can explore these if you need more technical details later. For now, focus on downloading and running the API on your Windows PC.

---

## 🔄 Updating ECommerce-Api

To update, repeat the download process with the latest release from the page. Extract and replace the old folder with the new one. Start the program again in Command Prompt following the previous steps.

---

## 🧰 Additional Tips

- Keep the extracted folder in a place you can find easily, like Documents or Desktop.  
- If you want to stop the API, close the Command Prompt window running the program.  
- You can learn basic web testing tools if you want to see API results directly in your browser.

---

[![Download Latest Release](https://img.shields.io/badge/Download-ECommerce--Api-blue?style=for-the-badge)](https://github.com/LucaRain/ECommerce-Api/releases)