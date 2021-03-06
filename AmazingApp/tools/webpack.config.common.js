var fs = require("fs");
var path = require("path");
var HtmlWebpackPlugin = require('html-webpack-plugin');
var HtmlWebpackPolyfillIOPlugin = require('html-webpack-polyfill-io-plugin');

var packageJson = JSON.parse(fs.readFileSync(resolve('../package.json')).toString());
var errorMsg = "{0} missing in package.json";

var config = {
  fable: {
    main: {
      fsproj: resolve(path.join("..", "src/AmazingApp.fsproj")),
      output: 'main.js'
    },
    serviceWorker: {
      fsproj: resolve(path.join("..", "ServiceWorker/ServiceWorker.fsproj")),
      output: 'sw.js'
    }
  },
  publicDir: resolve("../public"),
  buildDir: resolve("../build"),
  nodeModulesDir: resolve("../node_modules"),
  indexHtmlTemplate: resolve("../src/index.html")
}

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

function forceGet(obj, path, errorMsg) {
  function forceGetInner(obj, head, tail) {
    if (head in obj) {
      var res = obj[head];
      return tail.length > 0 ? forceGetInner(res, tail[0], tail.slice(1)) : res;
    }
    throw new Error(errorMsg.replace("{0}", path));
  }
  var parts = path.split('.');
  return forceGetInner(obj, parts[0], parts.slice(1));
}

function getModuleRules(isProduction) {
  var babelOptions = {
    presets: [
        ["@babel/preset-env", {
            "targets": {
                "browsers": ["last 2 versions"]
            },
            "modules": false
        }]
    ]
};

  return [
    {
      test: /\.fs(x|proj)?$/,
      use: {
        loader: "fable-loader",
        options: {
          babel: babelOptions,
          define: isProduction ? [] : ["DEBUG"]
        }
      }
    },
    {
      test: /\.js$/,
      exclude: /node_modules/,
      use: {
        loader: 'babel-loader',
        options: babelOptions
      },
    },
    {
      test: /\.(sass|scss|css)$/,
      use: [
        "style-loader",
        "css-loader",
        "sass-loader"
      ]
    },
    {
      test: /.(ttf|otf|eot|svg|woff(2)?)(\?[a-z0-9]+)?$/,
      use: [{
        loader: 'file-loader',
        options: {
          name: '[name].[ext]',
          outputPath: 'fonts/',       // override the default path
          publicPath: 'fonts/'       // override the default path
        }
      }]
    }
  ];
}

function getPlugins(isProduction) {
  return [
    new HtmlWebpackPlugin({
      filename: path.join(config.buildDir, "index.html"),
      template: config.indexHtmlTemplate,
      // minify: isProduction ? {} : false
    }),
    new HtmlWebpackPolyfillIOPlugin({ features: "es6,fetch" }),
    // new DynamicCdnWebpackPlugin({ verbose: true, only: config.cdnModules }),
  ];
}

module.exports = {
  resolve: resolve,
  config: config,
  getModuleRules: getModuleRules,
  getPlugins: getPlugins
}
