[Code]
type
	NetFXType = (NetFx10, NetFx11, NetFx20, NetFx30, NetFx35, NetFx40Client, NetFx40Full, NetFx4x);

const
	netfx11plus_reg = 'Software\Microsoft\NET Framework Setup\NDP\';

function dotnetfxinstalled(version: NetFXType; lcid: String): Boolean;
var
	regVersion: Cardinal;
	regVersionString: String;
begin
	if (lcid <> '') then
		lcid := '\' + lcid;

	case version of
		NetFx10:
			Result := RegQueryStringValue(HKLM, 'Software\Microsoft\.NETFramework\Policy\v1.0\3705', 'Install', regVersionString) and (regVersionString <> '');
		NetFx11:
			Result := RegQueryDWordValue(HKLM, netfx11plus_reg + 'v1.1.4322' + lcid, 'Install', regVersion) and (regVersion <> 0);
		NetFx20:
			Result := RegQueryDWordValue(HKLM, netfx11plus_reg + 'v2.0.50727' + lcid, 'Install', regVersion) and (regVersion <> 0);
		NetFx30:
			Result := RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.0\Setup' + lcid, 'InstallSuccess', regVersion) and (regVersion <> 0);
		NetFx35:
			Result := RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.5' + lcid, 'Install', regVersion) and (regVersion <> 0);
		NetFx40Client:
			Result := RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Client' + lcid, 'Install', regVersion) and (regVersion <> 0);
		NetFx40Full:
			Result := RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Install', regVersion) and (regVersion <> 0);
		NetFx4x:
			Result := RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion) and (regVersion >= 378389); // 4.5.0+
	end;
end;

function dotnetfxspversion(version: NetFXType; lcid: String): Integer;
var
	regVersion: Cardinal;
begin
	if (lcid <> '') then
		lcid := '\' + lcid;

	case version of
		NetFx10:
			// not supported
			regVersion := -1;
		NetFx11:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v1.1.4322' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx20:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v2.0.50727' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx30:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.0' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx35:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v3.5' + lcid, 'SP', regVersion)) then
				regVersion := -1;
		NetFx40Client:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Client' + lcid, 'Servicing', regVersion)) then
				regVersion := -1;
		NetFx40Full:
			if (not RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Servicing', regVersion)) then
				regVersion := -1;
		NetFx4x:
			if (RegQueryDWordValue(HKLM, netfx11plus_reg + 'v4\Full' + lcid, 'Release', regVersion)) then begin
				if (regVersion >= 528040) then
					regVersion := 80 // 4.8.0+ 
				else if (regVersion >= 461808) then
					regVersion := 72 // 4.7.2+
				else if (regVersion >= 461308) then
					regVersion := 71 // 4.7.1+
				else if (regVersion >= 460798) then
					regVersion := 70 // 4.7.0+
				else if (regVersion >= 394802) then
					regVersion := 62 // 4.6.2+
				else if (regVersion >= 394254) then
					regVersion := 61 // 4.6.1+
				else if (regVersion >= 393295) then
					regVersion := 60 // 4.6.0+
				else if (regVersion >= 379893) then
					regVersion := 52 // 4.5.2+
				else if (regVersion >= 378675) then
					regVersion := 51 // 4.5.1+
				else if (regVersion >= 378389) then
					regVersion := 50 // 4.5.0+
				else
					regVersion := -1;
			end;
	end;
	Result := regVersion;
end;
