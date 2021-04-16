{ pkgs ? import <nixpkgs> { } }:

let
  datasetteVega = with pkgs.python39.pkgs; buildPythonPackage rec {
    pname = "datasette-vega";
    version = "0.6.2";
    format = "wheel";

    src = fetchPypi (
      {
        pname = "datasette_vega";
        inherit version format;
        python = "py3";
        sha256 = "sha256:0qx005x9yf0d3wjdx6mqsmmzj3lzm6agzgs8ap8dci5jy106mjka";
      });

    buildInputs = [ pkgs.python39Packages.datasette ];
    propagatedBuildInputs = [ setuptools ];

    meta = with pkgs.lib; {
      description = "A Datasette plugin that provides tools for generating charts using Vega.";
      homepage = "https://github.com/simonw/datasette-vega";
      license = licenses.asl20;
    };
  };

  customPython = with pkgs; python39.buildEnv.override {
    extraLibs = [
      python39Packages.datasette
      python39Packages.sqlite-utils
      datasetteVega
    ];
  };
in
pkgs.mkShell {
  buildInputs = with pkgs; [
    customPython
    sqlite
    jq
  ];
}
