import React from 'react'
import ReactDOM from 'react-dom'
import App from './App'
import './sass/style.css'
import registerServiceWorker from './registerServiceWorker'
import { Provider } from 'react-redux'
import Modal from 'react-modal'
import { IntlProvider, addLocaleData } from 'react-intl'
import 'core-js/fn/string/includes'
import 'core-js/es7/array'
import './polyfills'

import setLocaleStorage from './setLocaleStorage'
import configureStore from './store/configureStore'
import { changeLocale } from './actions/configActions'

import messages from './i18n/translated-messages.json'


Modal.setAppElement('#new-validation-module-root');

const store = configureStore();

setLocaleStorage(store);

const rootElement = document.getElementById('reporting-service');
const uid = rootElement && rootElement.dataset.uid;

const pathArr = window.location.pathname.split('/');
let locale = pathArr && pathArr[1];
locale && store.dispatch(changeLocale(locale));
const formattedLocale = locale && locale.replace('_', '-');

ReactDOM.render((
    <IntlProvider
        locale={formattedLocale}
        defaultLocale={'en-US'}
        messages={messages[locale]}
    >
        <Provider store={store}>
            <App uid={uid} />
        </Provider>
    </IntlProvider>
), rootElement);
registerServiceWorker();