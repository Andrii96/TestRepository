import * as types from '../constants/actionTypes'
import request from '../httpConfig'

const getAssigneeListByTaskIdSuccess = (assigneeList, taskId) => (
    {
        type: types.GET_ASSIGNEE_LIST_BY_TASK_SUCCESS,
        assigneeList,
        taskId
    }
);

const loadAssigneeListByTaskId = taskId => (
    async dispatch => {
        try {
            const res = await request.get(`/api/TranslationValidationTasks/validators?taskId=${taskId}`);
            const data = res.data;
            dispatch(getAssigneeListByTaskIdSuccess(data, taskId));
            return data;
        } catch (e) { throw e.response && e.response.data }
    }
);

export {
    loadAssigneeListByTaskId
}