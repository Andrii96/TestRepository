/* global process */
import { createStore, applyMiddleware } from 'redux'
import reduxImmutableStateInvariant from 'redux-immutable-state-invariant'
import thunk from 'redux-thunk'
import rootReducer from '../reducers'
import { composeWithDevTools } from 'redux-devtools-extension'

const configureStoreProd = initialState => {
    const middlewares = [
        thunk
    ];
    return createStore(
        rootReducer,
        initialState,
        applyMiddleware(...middlewares)
    );
};

const configureStoreDev = initialState => {
    const middlewares = [
        reduxImmutableStateInvariant(),
        thunk
    ];
    return createStore(
        rootReducer,
        initialState,
        composeWithDevTools(applyMiddleware(...middlewares))
    );
};

const configureStore = process.env.NODE_ENV === 'production' ? configureStoreProd : configureStoreDev;

export default configureStore;