import * as types from '../constants/actionTypes'
import request from '../httpConfig'

const getAllFilteredTasksSuccess = groupedTasksList => (
    {
        type: types.GET_ALL_FILTERED_TASKS_SUCCESS,
        groupedTasksList
    }
);

const changeLoadedTaskStatus = isLoaded => (
    {
        type: types.CHANGE_LOADED_TASK_STATUS,
        isLoaded
    }
);

const loadFilesForTaskSuccess = (taskId, data) => (
    {
        type: types.LOAD_FILES_FOR_TASK_SUCCESS,
        taskId,
        data
    }
);

const reassignTaskSuccess = task => (
    {
        type: types.REASSIGN_TASK_SUCCESS,
        task
    }
);

const finalizeTaskSuccess = task => (
    {
        type: types.FINALIZE_TASK_SUCCESS,
        task
    }
);

const getAllTasksByFilter = data => (
    async dispatch => {
        dispatch(changeLoadedTaskStatus(false));
        try {
            const res = await request.post('/api/TranslationValidationTasks/paged', data);
            dispatch(getAllFilteredTasksSuccess(res.data && res.data.Tasks));
            dispatch(changeLoadedTaskStatus(true));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const loadFilesForTask = id => (
    async dispatch => {
        try {
            const res = await request.get(`/api/TranslationValidationTasks/files?taskId=${id}`);
            dispatch(loadFilesForTaskSuccess(id, res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const reassignTask = ({taskId, contactId}) => (
    async dispatch => {
        try {
            const res = await request.post(`/api/TranslationValidationTasks/reassign?taskId=${taskId}&contactId=${contactId}`);
            dispatch(reassignTaskSuccess(res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const finalizeTask = taskId => (
    async dispatch => {
        try {
            const res = await request.get(`/api/TranslationValidationTasks/finalizeTask?taskId=${taskId}`);
            dispatch(finalizeTaskSuccess(res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);


export {
    getAllTasksByFilter,
    loadFilesForTask,
    reassignTask,
    finalizeTask
}