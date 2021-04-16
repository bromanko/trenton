{ pkgs ? import <nixpkgs> { } }:

let
  datasetteVega = pkgs.python39.pkgs.buildPythonPackage rec {
    pname = "datasette-vega";
    version = "0.6.2";
    format = "wheel";

    src = pkgs.python39.pkgs.fetchPypi (
      {
        pname = "datasette_vega";
        inherit version format;
        python = "py3";
        sha256 = "sha256:0qx005x9yf0d3wjdx6mqsmmzj3lzm6agzgs8ap8dci5jy106mjka";
      });

    buildInputs = [ pkgs.python39Packages.datasette ];
    propagatedBuildInputs = [ pkgs.python39.pkgs.setuptools ];

    meta = with pkgs.lib; {
      description = "A Datasette plugin that provides tools for generating charts using Vega.";
      homepage = "https://github.com/simonw/datasette-vega";
      license = licenses.asl20;
    };
  };

  customPython = pkgs.python39.buildEnv.override {
    extraLibs = [
      pkgs.python39Packages.datasette
      pkgs.python39Packages.sqlite-utils
      datasetteVega
    ];
  };
in
pkgs.mkShell {
  buildInputs = [
    customPython
    pkgs.sqlite
    pkgs.jq
  ];
}
