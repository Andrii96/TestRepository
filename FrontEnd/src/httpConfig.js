import axios from 'axios'
import CONFIG from './config'

const pathArr = window.location.pathname.split('/');
let locale = pathArr && pathArr[1];
const formattedLocale = locale && locale.replace('_', '-');

const rootElement = document.getElementById('new-validation-module-root');
const uid = rootElement && rootElement.dataset.uid;

const headers = {
    'Accept-Language': formattedLocale,
    'ContactId': uid
};

const request = axios.create({
    baseURL: CONFIG.apiBaseUrl,
    headers: headers
});

export default request