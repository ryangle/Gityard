# Gityard

使用LibGit2Sharp实现的简单的Git服务器。

如果没有太多的功能要求，可以试试本项目。

得益于.Net的跨平台特性，本项目可以部署在
Windows、Linux、MacOS等系统上。

## 引用了以下项目

* 数据库[LiteDb](https://www.litedb.org/)
* [Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
* 日志[Serilog](https://serilog.net/)
* [LibGit2Sharp](https://github.com/libgit2/libgit2sharp/)


## 部署时需要：

* 安装git，以支持git命令
* 设置appsettings.json中的GityardOptions，BasePaht为服务器的仓库目录