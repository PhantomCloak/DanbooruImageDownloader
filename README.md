# Danbooru Image Downloader
Simple danbooru image downloader that allows you to download vast amount of images realted to specified tag 


## How to use
<br/>

Example command that downloads images tagged with hatsune miku 
```
./DanbooruImageDownloader --tag hatsune_miku --page-limit 1
```
### **For windows users that doesn't want to do magic tricks**
Download the interactive version of the program under the Download section

<br/>

### Essential commands
<br/>

`--tag` - Desired tag

`--page-limit` - Number of pages to download

<br/>


### Optional commands
<br/>

`--save-path` - Save path for downloaded images 

`--proxy-address` - HTTP proxy address and port such as `http://0.0.0.0:80`

`--proxy-username` - Username for specified proxy if it exist

`--proxy-password` - Password for specified proxy if it exist 

`--compressed-only` - Does not attempt to download the original size of the scaled image

<br/>
Full example command that uses proxy

```
./DanbooruImageDownloader --tag hatsune_miku --page-limit 1 --save-path ../img --proxy-address http://0.0.0.:65233 --proxy-username foo --proxy-password bar
```
<br/>

## Download
<br/>

### For Linux [Here](https://github.com/PhantomCloak/DanbooruImageDownloader/releases/download/1.0/linux-x64.zip)
<br/>

### For Windows [Here](https://github.com/PhantomCloak/DanbooruImageDownloader/releases/download/1.0/windows-x64.zip)
<br/>

### For Windows (Interactive) [Here](https://github.com/PhantomCloak/DanbooruImageDownloader/releases/download/1.0/windows-interactive.zip)
<br/>

### For MacOS [Here](https://github.com/PhantomCloak/DanbooruImageDownloader/releases/download/1.0/osx-x64.zip)
<br/>

## Build from source
<br/>

Pull from repository
```
git clone https://github.com/PhantomCloak/DanbooruImageDownloader.git
```
<br/>

Go to the source directory and run following commands
```
cd DanbooruIageDownloader

dotnet restore

dotnet build
```
<br/>

### Build Dependencies
- .NET Core 3.1.403 SDK


