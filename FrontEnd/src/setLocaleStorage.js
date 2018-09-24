import * as types from './constants/actionTypes'

const setLocaleStorage = store => {
    const tableCols = JSON.parse(localStorage.getItem('tableCols'));
    if (tableCols) store.dispatch({type: types.ON_SORT_COLUMN_END, tableCols});

    // const openedArray = JSON.parse(localStorage.getItem('openedArray'));
    // if (openedArray) store.dispatch({type: types.CHANGE_OPENED_TASKS, openedArray});

    const matches = JSON.parse(localStorage.getItem('matches'));
    if (matches) {
        const selectedMatches = matches.filter(m => m.selected);
        const selectedMatchesValue = selectedMatches && selectedMatches[0] && selectedMatches[0].value;
        if (selectedMatchesValue) store.dispatch({type: types.CHANGE_SORT_MATCHES, name: 'matches', value: selectedMatchesValue});
    }

    const percentage = JSON.parse(localStorage.getItem('percentage'));
    if (percentage) {
        const selectedPercentage = percentage.filter(p => p.selected);
        const selectedPercentageValue = selectedPercentage && selectedPercentage[0] && selectedPercentage[0].value;
        if (selectedPercentageValue) store.dispatch({type: types.CHANGE_SORT_MATCHES, name: 'percentage', value: selectedPercentageValue});
    }
};

export default setLocaleStorage