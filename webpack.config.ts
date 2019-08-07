import * as MiniCssExtractPlugin from 'mini-css-extract-plugin';
import * as OptimizeCSSAssetsPlugin from 'optimize-css-assets-webpack-plugin';
import * as path from 'path';
import * as TerserPlugin from 'terser-webpack-plugin';
import * as webpack from 'webpack';

interface IEnv {
  production: boolean;
}

const sitePath = path.resolve(__dirname, 'Sources', 'EPiServer.Reference.Commerce.Site');
const featurePath = path.resolve(sitePath, 'Features');
const outputPath = path.resolve(sitePath, 'Scripts', 'assets');

const resolve: webpack.Resolve = {
  extensions: ['.ts', '.tsx', '.js', '.jsx'],
  alias: {
    features: featurePath,
  }
}

const output: webpack.Output = {
  publicPath: '/scripts/assets/',
  filename: '[name].bundle.js',
  chunkFilename: '[name].bundle.js',
  path: outputPath,
};

const entry: webpack.Entry = {
  main: [path.resolve(featurePath, 'index')],
};

const optimization = (env: IEnv): webpack.Options.Optimization => ({
  minimize: env.production,
  splitChunks: {
    cacheGroups: {
      vendor: {
        test: /[\\/]node_modules[\\/]/,
        name: 'vendor',
        chunks: 'initial',
        enforce: true,
        reuseExistingChunk: true,
      },
    },
  },
  minimizer: [
    new TerserPlugin({
      sourceMap: true,
      parallel: true,
      cache: true,
      extractComments: 'all',
    }),
    new OptimizeCSSAssetsPlugin({}),
  ],
});

const plugins = (env: IEnv): webpack.Plugin[] =>
  [
    new webpack.DefinePlugin({
      isProd: env.production,
      'process.env': {
        ['NODE_ENV']: JSON.stringify(
          env.production ? 'production' : 'development',
        ),
      },
    }),
    new MiniCssExtractPlugin({
      filename: '[name].css',
      chunkFilename: '[id].css',
    }),
    new webpack.LoaderOptionsPlugin({
      minimize: env.production,
      debug: !env.production,
    }),
  ].concat(
    env.production
      ? []
      : [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin(),
      ],
  );

const cssLoaders = (env: IEnv) => {
  let devLoader: string[] = [];
  if (!env.production) {
    devLoader = ['css-hot-loader'];
  }

  return [
    ...devLoader,
    MiniCssExtractPlugin.loader,
    {
      loader: 'css-loader',
      options: {
        modules: false,
        importLoaders: 2,
      },
    },
    {
      loader: 'postcss-loader',
      options: {
        ident: 'postcss',
        plugins: () => [
          require('postcss-import')(),
          require('postcss-url')(),
          require('autoprefixer')({ grid: true }),
          require('postcss-svg')(),
          require('postcss-browser-reporter')(),
          require('postcss-reporter')(),
        ],
      },
    },
    {
      loader: 'sass-loader',
      options: {
        modules: false,
        importLoaders: 3,
      },
    },
  ];
};

const module = (env: IEnv): webpack.Module => ({
  rules: [
    {
      test: /\.(ts|tsx|js|jsx)?$/,
      use: {
        loader: 'ts-loader',
        options: {
          transpileOnly: true,
        },
      },
    },
    {
      test: /\.(sc|c|sa)ss$/,
      use: cssLoaders(env),
    },
    {
      test: /\.(otf|eot|ttf|svg|woff|woff2)$/,
      use: [
        {
          loader: 'file-loader',
          options: {
            name: 'fonts/[name].[ext]?[hash]',
          },
        },
      ],
    },
    {
      test: /\.(png|jpg|jpeg|gif)$/,
      use: [
        {
          loader: 'file-loader',
          options: {
            name: 'images/[name].[ext]?[hash]',
          },
        },
      ],
    },
  ],
});


const stats: webpack.Stats.ToJsonOptionsObject = {
  errorDetails: true,
  errors: true,
  chunks: true,
};

const config = (env: IEnv): webpack.Configuration => {
  env = env || { production: false };
  return {
    mode: env.production ? 'production' : 'development',
    resolve,
    entry,
    optimization: optimization(env),
    plugins: plugins(env),
    module: module(env),
    output,
    stats,
  };
};

export default config;