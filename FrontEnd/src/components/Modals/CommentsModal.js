import React, { Component } from 'react'
import Modal from 'react-modal'
import PropTypes from 'prop-types'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import moment from 'moment'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'

// Parts
import modalStyles from './modalStyles'
import * as filesActions from '../../actions/filesActions'
import generalMessages from '../../i18n/generalMessages'
import CommentIcon from '../svgIcons/CommentIcon'

const messages = defineMessages({
    enterCommentHint: {
        id: 'commentModal.enterCommentHint',
        defaultMessage: 'Enter comment'
    }
});

class CommentsModal extends Component {
    constructor(props){
        super(props);
        this.state = {
            creatingComment: false,
            isLoadingComments: !!this.props.rowForComment
        };

        this.onFieldChange = this.onFieldChange.bind(this);
        this.saveComment = this.saveComment.bind(this);
    }

    componentDidMount() {
        const {filesActions, rowForComment} = this.props;

        if(rowForComment) {
            const rowId = rowForComment.UnitId;
            const fileId = rowForComment.FileId;

            this.setState({isLoadingComments: true});
            filesActions.getCommentsByRow(fileId, rowId)
                .then(comments => this.setState({comments, isLoadingComments: false}))
                .catch(() => this.setState({isLoadingComments: false}));
        }
    }

    saveComment() {
        const {filesActions, currentUser: {ContactId}, rowForComment, targetFileId, intl: {formatMessage}} = this.props;
        const {userComment} = this.state;
        const {FileId, UnitId: RowId} = (rowForComment || {});

        if (!userComment) return this.setState({errorMessage: formatMessage(messages.enterCommentHint)});

        const data = {
            ContactId,
            FileId: FileId || targetFileId,
            RowId,
            Text: userComment
        };

        this.setState({creatingComment: true});
        filesActions.createComment(data)
            .then(res => {
                this.setState({userComment: '', creatingComment: false});
                this.props.onAddComment(res);
                this.props.closeModal();
            })
            .catch(e => {
                this.setState({creatingComment: false});
                console.error(e)
            })
    }

    onFieldChange(type) {
        return e => {
            this.setState({
                [type]: e.target.value,
                errorMessage: null
            })
        }
    }

    render() {
        const {closeModal, allComments, rowForComment, intl: {formatMessage}} = this.props;
        const {userComment, creatingComment, errorMessage, comments, isLoadingComments} = this.state;
        let modalStyle = window.innerWidth < 768 ? modalStyles.mobile : modalStyles.desktop;

        const content = Object.assign({}, modalStyle.content, {width: '750px'});
        modalStyle = Object.assign({}, modalStyle, {content});

        const commentsList = rowForComment ? comments : allComments;

        return (
            <Modal
                isOpen={true}
                contentLabel="tl-modal"
                style={modalStyle}
            >
                <div className="tl-modal-container">
                    <div className="tl-modal-header">
                        <FormattedMessage
                            id="commentModal.modalTitle"
                            defaultMessage="Comment"
                            tagName="h4"
                        />
                        <button onClick={closeModal}>×</button>
                    </div>
                    <div className="tl-modal-body tl-comment-modal">

                        <ul>
                            <FormattedMessage
                                id="commentModal.commentHint1"
                                defaultMessage="Comments are for your use only (internal communications, reminders, etc.)"
                                tagName="li"
                            />
                            <FormattedMessage
                                id="commentModal.commentHint2"
                                defaultMessage="Comments will not be taken into consideration by Telelingua before finalization of the files"
                                tagName="li"
                            />
                            <FormattedMessage
                                id="commentModal.commentHint3"
                                defaultMessage="Do not use comments to give instructions to Telelingua"
                                tagName="li"
                            />
                        </ul>

                        <textarea
                            name="comment"
                            onChange={this.onFieldChange('userComment')}
                            value={userComment}
                        ></textarea>

                        <div className="tl-clearfix">
                            <button
                                className="tl-primary-button tl-add-comment"
                                onClick={this.saveComment}
                                disabled={creatingComment}
                            >
                                <FormattedMessage
                                    id="commentModal.saveComment"
                                    defaultMessage="Save comment"
                                />
                            </button>
                            {creatingComment && <img
                                src="/img/loading.gif"
                                className="tl-loading-gif"
                                alt={formatMessage(generalMessages.loading)}
                                style={{float: 'right', margin: '4px 10px'}}
                            />}
                        </div>

                        {errorMessage && <p className="tl-error-message tl-text-right">{errorMessage}</p>}

                        {isLoadingComments && <div>
                            <h3 style={{display: 'flex', alignItems: 'center', margin: '0 0 15px'}}>
                                <img
                                    src="/img/loading.gif"
                                    className="tl-loading-gif"
                                    alt={formatMessage(generalMessages.loading)}
                                    style={{marginRight: 10}}
                                />
                                alt={formatMessage(generalMessages.loading)}
                            </h3>
                        </div>}

                        {!isLoadingComments && commentsList && commentsList[0] && <ul className="tl-modal-comment-list">
                            {commentsList.map(c => (
                                <li key={c.Id}>
                                    <span className="tl-comment-name">
                                        <CommentIcon
                                            width={12}
                                            height={12}
                                            fill={'#c50008'}
                                        />
                                        <strong>{c.Creator && c.Creator.Name}</strong>
                                        <span className="tl-comment-date">{moment(c.CreatedTime).format('YYYY-MM-DD @ HH:mm')}</span>
                                    </span>
                                    <span>{c.Text}</span>
                                </li>
                            ))}

                        </ul>}

                    </div>
                </div>


            </Modal>
        );
    }
}

CommentsModal.propTypes = {
    closeModal: PropTypes.func.isRequired,
    filesActions: PropTypes.object.isRequired,
    currentUser: PropTypes.object.isRequired,
    onAddComment: PropTypes.func.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        currentUser: state.currentUser
    }
}

function mapDispatchToProps(dispatch) {
    return {
        filesActions: bindActionCreators(filesActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(CommentsModal))