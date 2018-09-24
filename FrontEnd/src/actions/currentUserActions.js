import * as types from '../constants/actionTypes'
import axios from 'axios'

const getCurrentUserSuccess = user => (
    {
        type: types.GET_CURRENT_USER_SUCCESS,
        user
    }
);

const loadCurrentUser = (uid, locale) => (
    async dispatch => {
        try {
            const isDev = process.env.NODE_ENV === 'development' || window.location.host === '193.93.216.233:8072';
            const res = await axios.get(`${isDev ? 'http://193.93.217.155:8081/' : '/'}${locale}/validate/index/udata?uid=${uid}`);
            const data = res.data && res.data.data;
            dispatch(getCurrentUserSuccess(data));
            return data;
        } catch (e) { throw e.response && e.response.data }
    }
);

export {
    loadCurrentUser
}