// requires Windows 10 Version 1607+, Windows 7 SP1+, Windows 8.1, Windows Server 2012 R2
// https://dotnet.microsoft.com/download/dotnet-core/3.1

[CustomMessages]
netcore31desktop_title=.NET Desktop Runtime 3.1.6 (x86)
netcore31desktop_title_x64=.NET Desktop Runtime 3.1.6 (x64)

netcore31desktop_size=23 MB
netcore31desktop_size_x64=26 MB

[Code]
const
	netcore31desktop_url = 'http://go.microsoft.com/fwlink/?linkid=2137844';
	netcore31desktop_url_x64 = 'http://go.microsoft.com/fwlink/?linkid=2137941';

procedure netcore31desktop();
begin
	if (not IsIA64()) then begin
		if not netcoreinstalled(Desktop, '3.1.6') then
			AddProduct('netcore31desktop' + GetArchitectureString() + '.exe',
				'/lcid ' + CustomMessage('lcid') + ' /passive /norestart',
				CustomMessage('netcore31desktop_title' + GetArchitectureString()),
				CustomMessage('netcore31desktop_size' + GetArchitectureString()),
				GetString(netcore31desktop_url, netcore31desktop_url_x64, ''),
				false, false, false);
	end;
end;
