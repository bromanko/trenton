{ pkgs ? import <nixpkgs> { } }:

let
  customPython = pkgs.python39.buildEnv.override {
    extraLibs = [ pkgs.python39Packages.datasette ];
  };
in
pkgs.mkShell {
  buildInputs = [
    customPython
  ];
}
