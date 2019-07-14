#Install chocolatey and configure it
if (!(Get-Command choco -ErrorAction SilentlyContinue)){
  iex (wget 'https://chocolatey.org/install.ps1' -UseBasicParsing)
  choco feature disable --name showDownloadProgress
}
#Install required tools
choco install -y vcredist-all monogame
#Open port 60321 on Windows Firewall
New-NetFirewallRule -DisplayName "Allow inbound TCP port 60321" -Direction inbound -LocalPort 60321 -Protocol TCP -Action Allow
#Add a rule to allow Vagrant user to host the server
if(!(netsh http show urlacl url=http://*:60321/api/Effect/  | Where-Object { $_ -match [regex]::Escape("http://*:60321/api/Effect/")}) ){
  netsh http add urlacl url=http://*:60321/api/Effect/ user=vagrant\vagrant
}

#Download and unzip serman
if (!(Test-Path "c:\serman\serman.exe")){
  if (!(Test-Path "c:\serman")){
    mkdir c:\serman
  }
  if (!(Test-Path "c:\serman.zip")){
    if (Test-Path "c:\vagrant\serman.zip"){
      Copy-Item "c:\vagrant\serman.zip" -Destination "c:\serman.zip"
    } else {
      Invoke-WebRequest -Uri "https://github.com/kflu/serman/releases/download/0.3.2/serman.zip" -OutFile "c:\serman.zip"
    }
  }
  Expand-Archive "c:\serman.zip" -DestinationPath "C:\serman"
  #injecting different winsw version because the one shipped with serman requires .net 3.5 which is a pain to install headless
  rm c:\serman\winsw.exe
  if (Test-Path "c:\vagrant\winsw.exe"){
    Copy-Item "c:\vagrant\winsw.exe" -Destination "c:\serman\winsw.exe"
  } else {
    Invoke-WebRequest -Uri "https://github.com/kohsuke/winsw/releases/download/winsw-v2.2.0/WinSW.NET4.exe" -OutFile "c:\serman\winsw.exe"
  }
}

#Finally installing our server :D
if (!(Test-Path "c:\MonoGameUniversalEffectsServer\MonoGameUniversalEffects.Server.exe")){
  if (!(Test-Path "c:\MonoGameUniversalEffectsServer")){
    mkdir c:\MonoGameUniversalEffectsServer
  }
  if (!(Test-Path "c:\MonoGameUniversalEffectsServer.zip")){
    if (Test-Path "c:\vagrant\server.zip"){
      Copy-Item "c:\vagrant\server.zip" -Destination "c:\MonoGameUniversalEffectsServer.zip"
    } else {
      Invoke-WebRequest -Uri "https://github.com/augustozanellato/MonoGameUniversalEffects/releases/latest/download/server.zip" -OutFile "c:\MonoGameUniversalEffectsServer.zip"
    }
  }
  Expand-Archive "c:\MonoGameUniversalEffectsServer.zip" -DestinationPath "C:\MonoGameUniversalEffectsServer"
}
c:\serman\serman.exe install "C:\MonoGameUniversalEffectsServer\MonoGameUniversalEffectsSever.xml"