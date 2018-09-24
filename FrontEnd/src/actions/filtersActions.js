import * as types from '../constants/actionTypes'
import request from '../httpConfig'

const getAllFilterUsersSuccess = usersList => (
    {
        type: types.GET_ALL_FILTER_USERS_SUCCESS,
        usersList
    }
);

const getAllFilterGroupsSuccess = groupsList => (
    {
        type: types.GET_ALL_FILTER_GROUPS_SUCCESS,
        groupsList
    }
);

const getAllFilterStatusSuccess = progressesList => (
    {
        type: types.GET_ALL_FILTER_STATUSES_SUCCESS,
        progressesList
    }
);

const changeFilter = (filterType, value) => (
    {
        type: types.CHANGE_FILTER,
        filterType,
        value
    }
);

const changeAllFilterList = (filterType, isChecked) => (
    {
        type: types.CHANGE_ALL_FILTER_LIST,
        filterType,
        isChecked
    }
);

const getAllUsers = data => (
    async dispatch => {
        try {
            const res = await request.post('/api/Contacts/allUsers', data);
            dispatch(getAllFilterUsersSuccess(res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const getAllGroups = data => (
    async dispatch => {
        try {
            const res = await request.get('/api/GroupColumns');
            dispatch(getAllFilterGroupsSuccess(res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const getAllStatus = data => (
    async dispatch => {
        try {
            const res = await request.get('/api/TaskStatus');
            dispatch(getAllFilterStatusSuccess(res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

export {
    getAllUsers,
    getAllGroups,
    getAllStatus,
    changeFilter,
    changeAllFilterList
}