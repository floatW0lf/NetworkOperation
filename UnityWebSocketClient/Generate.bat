mpc -i %~dp0/Assembly-CSharp.csproj -o %~dp0/Assets/GeneratedSerializer.cs
nodc --assemblies "%~dp0\Library\ScriptAssemblies\Assembly-CSharp.dll" --resolver "%~dp0\Library\ScriptAssemblies" --side Client --AOT -o %~dp0/Assets