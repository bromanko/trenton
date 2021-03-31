{ pkgs ? import <nixpkgs> { } }:
pkgs.mkShell {
  nativeBuildInputs = [
    pkgs.buildPackages.dotnet-sdk_5
    pkgs.buildPackages.dotnetCorePackages.aspnetcore_5_0
  ];
}
