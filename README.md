# JobEntry - Professional Job Portal & Recruitment Platform

**JobEntry** is a comprehensive job portal and recruitment web application built using **ASP.NET Core MVC**. It seamlessly connects job seekers with companies and employers, featuring advanced capabilities such as secure role-based authentication, profile management, and automated job synchronization from external platforms.

---

## 🚀 Key Features

* **Multi-Role Authentication & Authorization:**
  * Built using **ASP.NET Core Identity** with distinct user types (`UserType`) separating Job Seekers, Companies, and Administrators.
  * Robust route protection and role-based access control.
* **Smart Job Seeker & Company Profiles:**
  * Professional portfolio management and resume handling for job seekers.
  * Dedicated company dashboards for job postings, application tracking, and an admin company approval workflow (`IsApproved`).
* **Automated Data Integration (Apify Synchronization):**
  * Automated scraping and synchronization of job posts to keep the platform updated with diverse opportunities.
* **Robust Database Transactions:**
  * Powered by cloud-hosted **Azure SQL Server**, ensuring data integrity and transactional safety during user registrations and profile creations.

---

## 🛠️ Technology Stack

* **Backend:** ASP.NET Core MVC (.NET 8/9), C#
* **Database & ORM:** Entity Framework Core, Azure SQL Server
* **Security & Auth:** ASP.NET Core Identity, Anti-Forgery Tokens, Environment Variables for Secret Management
* **Cloud Hosting:** Microsoft Azure App Service (`jobentry-eg.me`)

---

## ⚙️ Security & Best Practices

* **Secret Management:** Sensitive credentials (such as Apify API Tokens) are completely decoupled from source code, utilizing **Azure Environment Variables** to prevent leaks (`GitHub Push Protection`).
* **Data Integrity:** Implements `BeginTransactionAsync` and `Rollback` during user registration and profile generation to prevent orphaned database records.

---

## 👨‍💻 Author
* **Ziad Jomaa** (Backend Architecture & Software Engineering)
