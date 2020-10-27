const qs = require('qs');
const axios = require('axios');
const { createProxyMiddleware } = require('http-proxy-middleware');
const baseURL = "http://commerceref/";
const getCommerceAuthToken = () => {
  const url = baseURL + 'token';

  const requestConfig = {
      headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
      }
  };

  const params = {
      grant_type: 'password',
      username: 'admin@example.com',
      password: 'store'
  };

  return axios.post(url, qs.stringify(params), requestConfig);
};

module.exports = function(app) {
  getCommerceAuthToken().then(response => {
    const commerceAuthToken = response.data.access_token;

    app.use('/api', createProxyMiddleware({
        target: baseURL + "episerver/Commerce/",
        changeOrigin: true,
        headers: {
          "Authorization": `Bearer ${commerceAuthToken}`
        }
    }));

    app.use('/api-extension', createProxyMiddleware({
      target: baseURL + "episerver/Commerce/",
      changeOrigin: true,
      headers: {
        "Authorization": `Bearer ${commerceAuthToken}`
      }
    }));

    app.use('/globalassets', createProxyMiddleware({
      target: baseURL,
      changeOrigin: true,
      headers: {
        "Authorization": `Bearer ${commerceAuthToken}`
      }
    }));
  })
};

