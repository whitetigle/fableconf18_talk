var webpack = require("webpack");
var common = require("./webpack.config.common");
var CopyWebpackPlugin = require('copy-webpack-plugin');

console.log("Bundling for production...");

function exports (entry) {
  return {
    entry: entry.fsproj,
    output: {
      filename: entry.output,
      path: common.config.buildDir,
    },
    module: {
      rules: common.getModuleRules()
    },
    plugins: common.getPlugins().concat([
      new CopyWebpackPlugin([ { from: common.config.publicDir } ])
    ]),
    resolve: {
      modules: [common.config.nodeModulesDir]
    },
  };
}

module.exports = [
  exports (common.config.fable.main),
  exports (common.config.fable.serviceWorker)
]