import React, { Component, Fragment } from 'react'
import Modal from 'react-modal'
import PropTypes from 'prop-types'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'
import { toastr } from 'react-redux-toastr'

// Parts
import modalStyles from './modalStyles'
import * as surveysByTaskIdActions from '../../actions/surveysByTaskIdActions'
import generalMessages from '../../i18n/generalMessages'

const messages = defineMessages({
    sendSurveySuccess: {
        id: 'surveyModal.sendSurveySuccess',
        defaultMessage: 'Thank you for your feedback'
    }
});

class QualitySurveyModal extends Component {
    constructor(props){
        super(props);
        this.state = {
            creatingSurvey: false,
            rates: {}
        };

        this.onHandleChanges = this.onHandleChanges.bind(this);
        this.onChangeSurvey = this.onChangeSurvey.bind(this);
        this.sendQualitySurvey = this.sendQualitySurvey.bind(this);
    }

    componentDidMount() {
        const { taskId, surveysByTaskIdActions } = this.props;

        surveysByTaskIdActions.loadSurveyByTaskId(taskId);
    }

    onHandleChanges(type) {
        return e => {
            this.setState({
                [type]: e.target.value
            })
        }
    }

    onChangeSurvey(categoryIndex, rowIndex, newPosition) {
        return e => {
            let {rates} = this.state;
            const checked = e.target.checked;

            const categoryObject = rates[categoryIndex];

            if (!categoryObject)
                rates = {...rates, [categoryIndex]: {}};

            rates = {
                ...rates,
                [categoryIndex]: {
                    ...categoryObject,
                    [rowIndex]: checked ? newPosition : -1
                }
            };

            this.setState({rates})
        }
    }

    sendQualitySurvey() {
        const {rates, comments} = this.state;
        const {surveysByTaskId, taskId, surveysByTaskIdActions, closeModal, intl: {formatMessage}} = this.props;

        const currentSurvey = surveysByTaskId[taskId];
        const categories = currentSurvey && currentSurvey.Categories;
        const rateMarks = [];

        categories.forEach((category, categoryIndex) => {
            category.Items && category.Items.forEach((row, rowIndex) => {
                const currentValue = rates[categoryIndex] && rates[categoryIndex][rowIndex];
                const value = (currentValue || currentValue === 0)
                    ?  currentValue
                    : row.Mark;
                rateMarks.push(value);
            })
        });

        const data = {
            TaskId: taskId,
            Comments: comments,
            Rates: rateMarks
        };

        this.setState({creatingSurvey: true});
        surveysByTaskIdActions.sendSurveyByTaskId(data)
            .then(() => {
                toastr.removeByType('success');
                toastr.success('', formatMessage(messages.sendSurveySuccess));
                closeModal();
            })
            .catch(() => {
                this.setState({creatingSurvey: false});
                toastr.removeByType('error');
                toastr.error('', formatMessage(generalMessages.defaultErrorMessage));
            })
    }

    checkChecked(categoryIndex, rowIndex, item, type) {
        const {rates} = this.state;
        const selectedInRow = rates[categoryIndex] && rates[categoryIndex][rowIndex];

        return ( (selectedInRow || selectedInRow === 0)
            ? rates[categoryIndex][rowIndex] === type.Position
            : type.Position === item.Mark)
    }


    render() {
        const {intl: {formatMessage}, closeModal, taskId, surveysByTaskId} = this.props;
        const {comments, creatingSurvey} = this.state;
        let modalStyle = window.innerWidth < 768 ? modalStyles.mobile : modalStyles.desktop;

        const currentSurvey = surveysByTaskId[taskId];

        return (
            <Modal
                isOpen={true}
                contentLabel="tl-modal"
                style={modalStyle}
            >
                <div className="tl-modal-container">
                    <div className="tl-modal-header">
                        <FormattedMessage
                            id="qualitySurveyModal.modalTitle"
                            defaultMessage="Quality Survey"
                            tagName="h4"
                        />
                        <button onClick={closeModal}>×</button>
                    </div>
                    <div className="tl-modal-body tl-survey-modal">

                        <ul className="tl-hints">
                            <FormattedMessage
                                id="qualitySurveyModal.surveyHint"
                                defaultMessage="Please provide us with your feedback on the points below."
                                tagName="li"
                            />
                        </ul>

                        {currentSurvey && <Fragment>

                            <div className="tl-survey-row tl-survey-header">
                                <div className="tl-survey-col tl-survey-name"></div>
                                {currentSurvey.MarkTypes && currentSurvey.MarkTypes.map((type, index) => (
                                    <div key={index} className="tl-survey-col">{type.Name}</div>
                                ))}
                            </div>

                            {currentSurvey.Categories && currentSurvey.Categories.map((category, categoryIndex) => (
                                <Fragment key={categoryIndex}>
                                    <div className="tl-survey-row tl-survey-sub-header">
                                        <div className="tl-survey-col tl-survey-name">{category.CategoryName}</div>
                                    </div>
                                    {category && category.Items && category.Items.map((item, rowIndex) => (
                                        <div key={rowIndex} className="tl-survey-row tl-survey-item">
                                            <div className="tl-survey-col tl-survey-name">{item.Name}</div>
                                            {currentSurvey.MarkTypes && currentSurvey.MarkTypes.map((type, index) => (
                                                <div key={index} className="tl-survey-col">
                                                    <input
                                                        id={`tl-survey-${categoryIndex}-${rowIndex}-${index}`}
                                                        type="checkbox"
                                                        checked={this.checkChecked(categoryIndex, rowIndex, item, type)}
                                                        onChange={this.onChangeSurvey(categoryIndex, rowIndex, type.Position)}
                                                    />
                                                    <label htmlFor={`tl-survey-${categoryIndex}-${rowIndex}-${index}`}>
                                                        <span></span>
                                                    </label>
                                                </div>
                                            ))}
                                        </div>
                                    ))}
                                </Fragment>
                            ))}

                            <div className="tl-survey-row tl-survey-sub-header">
                                <div className="tl-survey-col tl-survey-name">
                                    <FormattedMessage
                                        id="qualitySurveyModal.surveyCommentLabel"
                                        defaultMessage="Comment"
                                    />
                                </div>
                            </div>

                            <div className="tl-survey-row tl-survey-comment-row">
                                <div className="tl-survey-col">
                                    <textarea
                                        value={comments}
                                        defaultValue={currentSurvey && currentSurvey.Comments}
                                        onChange={this.onHandleChanges('comments')}
                                    ></textarea>
                                </div>
                                <div className="tl-survey-col">
                                    <div className="tk-survey-btn-wrap">
                                        {!creatingSurvey && <button
                                            onClick={this.sendQualitySurvey}
                                        >
                                            <FormattedMessage
                                                id="qualitySurveyModal.surveySaveButtonLabel"
                                                defaultMessage="Save"
                                            />
                                        </button>}
                                        {creatingSurvey && <div className="tl-loading-tasks">
                                            <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} />
                                            {formatMessage(generalMessages.loading)}
                                        </div>}
                                    </div>

                                </div>
                            </div>

                        </Fragment>}

                        {!currentSurvey && <div className="tl-loading-tasks">
                            <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} />
                            {formatMessage(generalMessages.loading)}
                        </div>}

                    </div>
                </div>


            </Modal>
        );
    }
}

QualitySurveyModal.propTypes = {
    closeModal: PropTypes.func.isRequired,
    surveysByTaskIdActions: PropTypes.object.isRequired,
    surveysByTaskId: PropTypes.object.isRequired,
    taskId: PropTypes.number.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        surveysByTaskId: state.surveysByTaskId
    }
}

function mapDispatchToProps(dispatch) {
    return {
        surveysByTaskIdActions: bindActionCreators(surveysByTaskIdActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(QualitySurveyModal))