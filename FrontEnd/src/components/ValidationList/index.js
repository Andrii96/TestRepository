import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import { Link } from 'react-router-dom'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import moment from 'moment'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'
import FileDownload from 'react-file-download'

import './ValidationList.css'
import SortablePanel from '../SortablePanel'
import * as filesActions from '../../actions/filesActions'
import * as configActions from '../../actions/configActions'
import * as groupedTasksActions from '../../actions/groupedTasksActions'
import * as assigneeListsByTaskActions from '../../actions/assigneeListsByTaskActions'
import generalMessages from '../../i18n/generalMessages'
import SortableIcon from '../svgIcons/SortableIcon'
import ToggleDetailsIcon from '../svgIcons/ToggleDetailsIcon'
import CommentIcon from '../svgIcons/CommentIcon'
import TLReassignSelectList from '../TLReassignSelectList'
import QualitySurveyModal from '../Modals/QualitySurveyModal'
import DeadlineWarningIcon from '../svgIcons/DeadlineWarningIcon'

const messages = defineMessages({
    unassignedCategory: {
        id: 'taskList.unassignedCategory',
        defaultMessage: 'Unassigned'
    }
});

class ValidationList extends Component {
    constructor(props) {
        super(props);
        this.state = {

        };

        this.toggleTask = this.toggleTask.bind(this);
        this.changeAssignee = this.changeAssignee.bind(this);
        this.finalizeTask = this.finalizeTask.bind(this);
        this.openQualitySurvey = this.openQualitySurvey.bind(this);
        this.closeModal = this.closeModal.bind(this);
    }

    toggleTask(id) {
        return () => {
            this.setState({loadingTasks: true});

            const {openedArray, configActions, groupedTasksActions, assigneeListsByTaskActions} = this.props;
            const index = openedArray.indexOf(id);
            configActions.changeOpenedTasks((index === -1) ? [id] : []);

            groupedTasksActions.loadFilesForTask(id)
                .then(files => this.setState({loadingTasks: false}))
                .catch(e => {
                    console.error(e);
                    this.setState({loadingTasks: false});
                });

            assigneeListsByTaskActions.loadAssigneeListByTaskId(id);
        };
    }

    toggleSortPanel(isOpenSortable) {
        return () => {
            this.props.configActions.toggleSortable(isOpenSortable)
        }
    }

    changeAssignee(taskId) {
        return contactId => {
            this.setState({assigneeProcess: true});
            this.props.groupedTasksActions.reassignTask({taskId, contactId})
                .finally(() => this.setState({assigneeProcess: false}));
        }
    }

    finalizeTask(taskId) {
        return () => {
            this.setState({finalizeProcess: true});
            this.props.groupedTasksActions.finalizeTask(taskId)
                .finally(() => this.setState({finalizeProcess: false}));
        }
    }

    openQualitySurvey(taskIdForSurvey) {
        return () => {
            this.setState({
                openQualitySurveyModal: true,
                taskIdForSurvey
            })
        }
    }

    closeModal() {
        this.setState({
            openQualitySurveyModal: false,
            taskIdForSurvey: null
        })
    }

    isDeadlinePassed(task) {
        if (task.IsFinalised) return false;
        return (+new Date(task.DeadLine)) < Date.now();
    }

    downloadPdf({fileId, taskId, name, isOpeningReferenceType}) {
        return () => {
            const {filesActions} = this.props;

            this.setState({openingPdf: fileId, isOpeningReferenceType});

            filesActions.downloadPdfReference({fileId, taskId})
                .then(res => FileDownload(res, name))
                .catch(e => console.log(e))
                .finally(() => this.setState({openingPdf: null, isOpeningReferenceType: null}))
        }
    }

    render() {
        const {loadingTasks, assigneeProcess, finalizeProcess, openingPdf, isOpeningReferenceType} = this.state;
        const {tableCols, groupedTasksList, loadedTasks, isOpenSortable, openedArray, locale,
            intl: {formatMessage}, assigneeListsByTaskId } = this.props;

        const visibleTableCols = tableCols.filter(col => col.visible);

        return (
            <div className="tl-validation-table">
                <div className="tl-table-header tl-table-row">
                    <div className="tl-table-col tl-col-sort">
                        <SortableIcon
                            width={12}
                            height={12}
                            onIconClick={this.toggleSortPanel(!isOpenSortable)}
                        />

                        <SortablePanel />
                    </div>
                    {visibleTableCols.map((column, index) =>(
                        <div className="tl-table-col" key={index}>
                            {formatMessage(generalMessages[column.name])}
                        </div>
                    ))}
                </div>

                {!loadedTasks && <div className="tl-loading-tasks">
                    <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} />
                    {formatMessage(generalMessages.loading)}
                </div>}

                {loadedTasks && !groupedTasksList[0] && <div className="tl-no-tasks">
                    {formatMessage(generalMessages.noItems)}
                </div>}

                {loadedTasks && groupedTasksList[0] && <div>
                    {groupedTasksList.map((item, index) => (
                        <div className="tl-table-group" key={index}>
                            <div className="tl-table-group-header">
                                {typeof item.Key === 'string' && item.Key}
                                {(typeof item.Key === 'object' && item.Key === null) && formatMessage(messages.unassignedCategory)}
                                {(typeof item.Key === 'object' && item.Key) && <div className="tl-sort-lang-titles">
                                    <FormattedMessage
                                        id="taskList.groupedLangTitle"
                                        defaultMessage="{source} >> {target}"
                                        values={{
                                            source: item.Key.SourceLanguage,
                                            target: item.Key.TargetLanguage
                                        }}
                                    />
                                </div>}
                            </div>

                            {item.Tasks && item.Tasks[0] && item.Tasks.map((task, index) => {
                                const isOpenDetails = openedArray.includes(task.TaskId);

                                return (<div key={index}>
                                    <div className={`tl-table-row ${isOpenDetails ? 'tl-open-row': ''}`} onClick={this.toggleTask(task.TaskId)}>
                                        <div className="tl-table-col tl-col-sort">
                                            <ToggleDetailsIcon
                                                isOpenDetails={isOpenDetails}
                                                width={24}
                                                height={24}
                                            />
                                        </div>
                                        {visibleTableCols.map((column, ind) => {
                                            const fieldsLength = column.fields.length;
                                            let name = '';
                                            if (fieldsLength === 1) name = task[column.fields[0]];
                                            if (fieldsLength === 2) name = task[column.fields[0]] ? task[column.fields[0]][column.fields[1]] : '';
                                            return (
                                                <div className="tl-table-col" key={ind}>
                                                    {column.isDate
                                                        ? column.fields.includes('DeadLine')
                                                            ? this.isDeadlinePassed(task)
                                                                ? <span className="tl-deadline-passed">{moment(name).format('YYYY-MM-DD')}<DeadlineWarningIcon
                                                                    width={16}
                                                                    height={16}
                                                                /></span>
                                                                : moment(name).format('YYYY-MM-DD')
                                                            : moment(name).format('YYYY-MM-DD')
                                                        : column.emailLink
                                                            ? <a
                                                                onClick={e => e.stopPropagation()}
                                                                href={`mailto:${task[column.fields[0]] ? task[column.fields[0]][column.emailLink]: ''}`}
                                                            >
                                                                {name}
                                                            </a>
                                                            : name
                                                    }
                                                </div>
                                            )
                                        })}
                                    </div>
                                    <div className={`tl-task-details ${isOpenDetails ? 'tl-open' : ''}`}>
                                        <div className="tl-task-inner">
                                            <div className="tl-task-top-row">
                                                {!task.IsFinalised && <Fragment>
                                                    {assigneeListsByTaskId[task.TaskId] && <Fragment>
                                                        <FormattedMessage
                                                            id="taskList.reassignTaskButton"
                                                            defaultMessage="Reassign this task"
                                                            tagName={'strong'}
                                                        />:
                                                        {assigneeProcess && <div className="tl-files-table-row" style={{margin: '4px 0 4px 10px'}}>
                                                            <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} />
                                                        </div>}
                                                        <TLReassignSelectList
                                                            selectedUserId={(task.Assignee && task.Assignee.ContactId) || 0}
                                                            assigneeGroups={assigneeListsByTaskId[task.TaskId]}
                                                            currentAssigneeId={task.Assignee && task.Assignee.ContactId}
                                                            onSelectAssignee={this.changeAssignee(task.TaskId)}
                                                        />
                                                    </Fragment>}
                                                    {!assigneeListsByTaskId[task.TaskId] && <div className="tl-files-table-row" style={{margin: '4px 0'}}>
                                                        <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} />
                                                        {formatMessage(generalMessages.loading)}
                                                    </div>}
                                                    {finalizeProcess && <div className="tl-files-table-row" style={{margin: '4px 0 4px 10px'}}>
                                                        <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} style={{marginRight: 0}} />
                                                    </div>}
                                                    <button
                                                        className="tl-button tl-green-button"
                                                        onClick={this.finalizeTask(task.TaskId)}
                                                        disabled={finalizeProcess}
                                                    >
                                                        <FormattedMessage
                                                            id="taskList.finalizeTaskButton"
                                                            defaultMessage="Finalize this task"
                                                        />
                                                    </button>
                                                </Fragment>}
                                                {task.IsFinalised && <Fragment>
                                                    <button
                                                        className="tl-button tl-blue-button"
                                                        onClick={this.openQualitySurvey(task.TaskId)}
                                                    >
                                                        <CommentIcon width={12} height={12} fill="#fff" />
                                                        <FormattedMessage
                                                            id="taskList.qualitySurveyButton"
                                                            defaultMessage="Quality survey"
                                                        />
                                                    </button>
                                                </Fragment>}
                                            </div>
                                        </div>
                                        <div className="tl-task-files-list">
                                            <div className="tl-files-left">
                                                <button>
                                                    <FormattedMessage
                                                        id="taskList.taskFilesButton"
                                                        defaultMessage="Files"
                                                    />
                                                </button>
                                            </div>
                                            <div className="tl-files-right-table">
                                                {task.filesList && task.filesList[0] && <div className="tl-files-table-row tl-files-table-header">
                                                    <div className="tl-files-col tl-task-name-col">
                                                        <FormattedMessage
                                                            id="taskList.fileLabel"
                                                            defaultMessage="File"
                                                        />
                                                    </div>
                                                    <div className="tl-files-col">
                                                        <FormattedMessage
                                                            id="taskList.fileDateLabel"
                                                            defaultMessage="Date"
                                                        />
                                                    </div>
                                                    <div className="tl-files-col">
                                                        <FormattedMessage
                                                            id="taskList.fileStatusLabel"
                                                            defaultMessage="Status"
                                                        />
                                                    </div>
                                                    <div className="tl-files-col"></div>
                                                </div>}

                                                {task.filesList && task.filesList[0] && task.filesList.map(record => (
                                                    <div key={record.RecordId} className="tl-files-table-row">
                                                        <div className="tl-files-col tl-task-name-col">{record.FileName}</div>
                                                        <div className="tl-files-col">{moment(record.StartDate).format('YYYY-MM-DD HH:MM:SS')}</div>
                                                        <div className="tl-files-col">{record.FileStatus}</div>
                                                        <div className="tl-files-col">
                                                            {record.IsOnline
                                                                ? <Link to={`/${locale}/validate/orc?validationTaskId=${record.TaskId}&fileId=${record.RecordId}`}>
                                                                    <FormattedMessage
                                                                        id="taskList.viewOnlineLink"
                                                                        defaultMessage="view online"
                                                                    />
                                                                </Link>
                                                                : <span
                                                                    className="tl-download-pdf-offline"
                                                                    onClick={this.downloadPdf({
                                                                        fileId: record.RecordId,
                                                                        taskId: task.TaskId,
                                                                        name: record.FileName,
                                                                        isOpeningReferenceType: 'record'
                                                                    })}
                                                                >
                                                                    {openingPdf === record.RecordId && isOpeningReferenceType === 'record' && <img
                                                                        src="/img/loading.gif"
                                                                        className="tl-loading-gif"
                                                                        alt={formatMessage(generalMessages.loading)}
                                                                    />}
                                                                    <FormattedMessage
                                                                        id="taskList.viewOffline"
                                                                        defaultMessage="view offline"
                                                                    />
                                                                </span>
                                                            }
                                                        </div>
                                                    </div>
                                                ))}

                                                {task.referencesList && task.referencesList[0] && <div className="tl-file-references-list">
                                                    <FormattedMessage
                                                        id="taskList.referencesHeader"
                                                        defaultMessage="References"
                                                        tagName="h5"
                                                    />
                                                    {task.referencesList.map( (reference, index) => (
                                                        <div key={index} className="tl-files-table-row">
                                                            <div className="tl-files-col tl-task-name-col">{reference.FileName} [{reference.FileSize}]</div>
                                                            <div className="tl-files-col">{moment(reference.Date).format('YYYY-MM-DD HH:MM:SS')}</div>
                                                            <div className="tl-files-col" />
                                                            <div className="tl-files-col">
                                                                <span
                                                                    className="tl-download-pdf-offline"
                                                                    onClick={this.downloadPdf({
                                                                        fileId: reference.FileId,
                                                                        taskId: task.TaskId,
                                                                        name: reference.FileName,
                                                                        isOpeningReferenceType: 'reference'
                                                                    })}
                                                                >
                                                                    {openingPdf === reference.FileId && isOpeningReferenceType === 'reference' && <img
                                                                        src="/img/loading.gif"
                                                                        className="tl-loading-gif"
                                                                        alt={formatMessage(generalMessages.loading)}
                                                                    />}
                                                                    <FormattedMessage
                                                                        id="taskList.downloadPdfLink"
                                                                        defaultMessage="download"
                                                                    />
                                                                </span>
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>}

                                                { (!task.filesList || !task.filesList[0]) && loadingTasks && <div className="tl-files-table-row" style={{margin: '4px 0'}}>
                                                    <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} />
                                                    {formatMessage(generalMessages.loading)}
                                                </div>}

                                                { (!task.filesList || !task.filesList[0]) && !loadingTasks && <div className="tl-files-table-row" style={{margin: '4px 0'}}>
                                                    <FormattedMessage
                                                        id="taskList.noFilesYet"
                                                        defaultMessage="There are no files yet."
                                                    />
                                                </div>}

                                                {/*<hr/>*/}

                                                {/*/!*Remove this fileLists:START*!/*/}
                                                {/*{filesList && filesList[0] && filesList.map(record => (*/}
                                                    {/*<div key={record.RecordId} className="tl-files-table-row">*/}
                                                        {/*<div className="tl-files-col tl-task-name-col">{record.FileName}</div>*/}
                                                        {/*<div className="tl-files-col">-</div>*/}
                                                        {/*<div className="tl-files-col">-</div>*/}
                                                        {/*<div className="tl-files-col">*/}
                                                            {/*<Link to={`/${locale}/validate/orc?validationTaskId=${record.TaskId}&fileId=${record.RecordId}`}>*/}
                                                                {/*<FormattedMessage*/}
                                                                    {/*id="taskList.viewOnlineLink"*/}
                                                                    {/*defaultMessage="view online"*/}
                                                                {/*/>*/}
                                                            {/*</Link>*/}
                                                        {/*</div>*/}
                                                    {/*</div>*/}
                                                {/*))}*/}
                                                {/*/!*Remove this fileLists:END*!/*/}

                                                {/*<hr/>*/}

                                            </div>
                                        </div>
                                    </div>
                                </div>)
                            })}
                        </div>
                    ))}
                </div>}

                {this.state.openQualitySurveyModal && <QualitySurveyModal
                    closeModal={this.closeModal}
                    taskId={this.state.taskIdForSurvey}
                />}

            </div>
        );
    }
}

ValidationList.propTypes = {
    filesActions: PropTypes.object.isRequired,
    configActions: PropTypes.object.isRequired,
    groupedTasksActions: PropTypes.object.isRequired,
    assigneeListsByTaskActions: PropTypes.object.isRequired,
    assigneeListsByTaskId: PropTypes.object.isRequired,
    groupedTasksList: PropTypes.array.isRequired,
    filesList: PropTypes.array.isRequired,
    loadedTasks: PropTypes.bool.isRequired,
    openedArray: PropTypes.array.isRequired,
    tableCols: PropTypes.array.isRequired,
    locale: PropTypes.string.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        currentUser: state.currentUser,
        filesList: state.filesList,
        groupedTasksList: state.groupedTasksList,
        loadedTasks: state.filters.loadedTasks,
        tableCols: state.config.tableCols,
        isOpenSortable: state.config.isOpenSortable,
        openedArray: state.config.openedArray,
        locale: state.config.locale,
        assigneeListsByTaskId: state.assigneeListsByTaskId
    }
}

function mapDispatchToProps(dispatch) {
    return {
        filesActions: bindActionCreators(filesActions, dispatch),
        configActions: bindActionCreators(configActions, dispatch),
        groupedTasksActions: bindActionCreators(groupedTasksActions, dispatch),
        assigneeListsByTaskActions: bindActionCreators(assigneeListsByTaskActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(ValidationList))