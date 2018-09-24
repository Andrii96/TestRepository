import React, { Component } from 'react'
import PropTypes from 'prop-types'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import { Link, Redirect } from 'react-router-dom'
import ReactPaginate from 'react-paginate'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'
import { toastr } from 'react-redux-toastr'
import FileDownload from 'react-file-download'

import * as filesActions from '../../actions/filesActions'
import * as configActions from '../../actions/configActions'
import SearchRecordsModal from '../Modals/SearchRecordsModal'
import CommentsModal from '../Modals/CommentsModal'
import './RecordView.css'
import ShowSegmentDiff from './ShowSegmentDiff'
import TLKeyboard from './TLKeyboard'
import generalMessages from '../../i18n/generalMessages'
import BookmarkIcon from '../svgIcons/BookmarkIcon'
import CommentIcon from '../svgIcons/CommentIcon'
import CommentArrowIcon from '../svgIcons/CommentArrowIcon'
import SearchIcon from '../svgIcons/SearchIcon'
import WarningIcon from '../svgIcons/WarningIcon'
import DiffIcon from '../svgIcons/DiffIcon'
import ToggleDetailsIcon from '../svgIcons/ToggleDetailsIcon'
import KeyboardIcon from '../svgIcons/KeyboardIcon'

const messages = defineMessages({
    noMatchSelectOption: {
        id: "recordSearchPanel.noMatchSelectOption",
        defaultMessage: "no match"
    },
    noMatch: {
        id: "recordView.noMatch",
        defaultMessage: "NoM"
    }
});

class RecordView extends Component {
    constructor(props){
        super(props);
        this.state = {
            openPanel: true,
            loadingDetails: true,
            loadingBookmarks: true,
            loadingComments: true,
            currentNavigation: 1,
            itemForDisplay: 100,
            addingMarkProgress: false,
            openedLogs: [],
            showDiff: false,
            onlyCorrection: false,
            openKeyboard: false
        };

        this.handlePageClick = this.handlePageClick.bind(this);
        this.showModal = this.showModal.bind(this);
        this.closeModal = this.closeModal.bind(this);
        this.checkHeaderWidth = this.checkHeaderWidth.bind(this);
        this.toggleSidebar = this.toggleSidebar.bind(this);
        this.onAddComment = this.onAddComment.bind(this);
        this.saveSegment = this.saveSegment.bind(this);
        this.cancelSegmentEditing = this.cancelSegmentEditing.bind(this);
        this.onModalReplace = this.onModalReplace.bind(this);
        this.toggleKeyboard = this.toggleKeyboard.bind(this);
        this.closeKeyboard = this.closeKeyboard.bind(this);
        this.openPdfWindowTab = this.openPdfWindowTab.bind(this);
        this.finalizeFile = this.finalizeFile.bind(this);
        this.downloadPdf = this.downloadPdf.bind(this);
        this.scrollElem = document.documentElement || document.body.parentNode || document.body;
    }

    componentDidMount() {
        const search = this.props.location.search;
        const searchArr = search && search.split('&');
        const searchFile = searchArr && searchArr[1];
        const searchFileArr = searchFile && searchFile.split('=');
        const recordId = searchFileArr && searchFileArr[1];

        const taskId = searchArr && searchArr[0] && searchArr[0].split('=') && searchArr[0].split('=')[1];

        const {filesActions, currentUser: {ContactId: contactId}, intl: { formatMessage }} = this.props;

        this.setState({taskId});

        filesActions.getFileDetails({recordId, contactId, taskId})
            .then(recordDetails => {
                if (this.taskPreview) this.setState({loadingDetails: false, recordDetails});

                const {FileId: fileId} = recordDetails;

                filesActions.downloadPdfReference({fileId, taskId})
                    .then(res => {
                        const blob = new Blob([new Uint8Array(res)]);
                        const pdfObjectURL = URL.createObjectURL(blob);
                        this.setState({pdfObjectURL})
                    }).catch(e => console.log(e))
            })
            .catch(e => {
                toastr.removeByType('error');
                toastr.error('', formatMessage(generalMessages.fileNotFoundNotification));
                this.setState({loadingDetails: false, redirectToList: true})
            });

        filesActions.loadBookmarks(recordId, contactId)
            .then(bookmarks => this.taskPreview ? this.setState({loadingBookmarks: false, bookmarks}) : null)
            .catch(e => this.setState({loadingBookmarks: false}));

        filesActions.loadComments(recordId)
            .then(comments => this.taskPreview ? this.setState({loadingComments: false, comments}) : null)
            .catch(e => this.setState({loadingComments: false}));

        this.checkHeaderWidth();

        window.addEventListener('resize', this.checkHeaderWidth)
    }

    componentWillUnmount() {
        window.removeEventListener('resize', this.checkHeaderWidth)
    }

    checkHeaderWidth() {
        if (!this.taskContainerNode || !this.panelNode) return;
        this.panelNode.style.width = `${this.taskContainerNode.clientWidth}px`;
    }

    handlePageClick(data) {
        this.scrollElem.scrollTop = 0;
        this.setState({currentNavigation: data.selected + 1});
    };

    showModal(name, row) {
        return () => {
            this.setState({[name]: true, rowForComment: row})
        }
    }

    closeModal() {
        this.setState({openSearchModal: false, openCommentsModal: false, rowForComment: null, openPdfModal: null})
    }

    addBookmark(row) {
        return () => {
            const {addingMarkProgress} = this.state;

            if (addingMarkProgress) return;

            const {filesActions, currentUser: {ContactId}} = this.props;
            const {FileId, UnitId: RowId, BookmarkId} = row;

            const replaceItems = newValue => {
                let recordDetails = this.state.recordDetails;
                let rows = recordDetails.Rows;
                rows = rows.map(row =>
                    row.UnitId === RowId
                        ? Object.assign({}, row, {BookmarkId: newValue})
                        : row
                );
                recordDetails = Object.assign({}, recordDetails, {Rows: rows});
                this.setState({recordDetails});
            };

            this.setState({addingMarkProgress: true});
            if (BookmarkId) {
                filesActions.deleteBookmark(BookmarkId)
                    .then(() => {
                        let bookmarks = this.state.bookmarks;
                        bookmarks = bookmarks.filter(b => b.Id !== BookmarkId);
                        this.setState({bookmarks});
                        replaceItems(null);
                        this.setState({addingMarkProgress: false});
                    }).catch(e => {
                        console.error(e);
                        this.setState({addingMarkProgress: false});
                    })
            } else {
                const data = {
                    ContactId,
                    FileId,
                    RowId
                };

                filesActions.createBookmark(data)
                    .then(bookmark => {
                        const bookmarks = this.state.bookmarks;
                        bookmarks.unshift(bookmark);
                        this.setState({bookmarks});
                        replaceItems(bookmark.Id);
                        this.setState({addingMarkProgress: false});
                    })
                    .catch(e => {
                        console.error(e);
                        this.setState({addingMarkProgress: false});
                    });
            }
        }
    }

    scrollToUnit(item) {
        return () => {
            const elem = document.getElementById(`tl-unit-${item.RowId}`);

            const goToElem = elem => {
                elem.scrollIntoView();
                window.scrollBy(0, -200);
                elem.style.animation = 'fadeInBgAnimation 3s';
                setTimeout(() => elem.style.animation = '', 3000)
            };

            if (!elem) {
                const {recordDetails} = this.state;
                if (!recordDetails) return;
                const rows = recordDetails.Rows;

                const selectedRow = rows.filter(r => r.UnitId === item.RowId);
                const index = rows.indexOf(selectedRow[0]);

                if (index === -1) return;

                const paginationPage = Math.ceil(index / 100) || 1;

                this.setState({currentNavigation: paginationPage}, () => {
                    const elem = document.getElementById(`tl-unit-${item.RowId}`);

                    if (!elem) return;

                    goToElem(elem);
                });

                return;
            }

            goToElem(elem);
        }
    }

    toggleSidebar() {
        this.setState({openPanel: !this.state.openPanel}, () => {
            let ms = 300;
            while (ms > 0) {
                setTimeout(this.checkHeaderWidth, ms);
                ms -= 10;
            }
        });
    }

    onAddComment(newComment) {
        const {comments} = this.state;
        comments.unshift(newComment);

        let recordDetails = this.state.recordDetails;
        let rows = recordDetails.Rows;
        rows = rows.map(row =>
            row.UnitId === newComment.RowId
                ? Object.assign({}, row, {CommentsCount: ++row.CommentsCount})
                : row
        );
        recordDetails = Object.assign({}, recordDetails, {Rows: rows});
        this.setState({comments, recordDetails});
    }

    onSelectChange(type) {
        return event => {
            this.props.configActions.changeSortMatches(type, event.target.value);
        }
    }

    applyMathFilter(file, filters) {
        let rows = file.Rows;
        let {matched, percentage} = filters;

        const checkRow = r => {
            switch (matched) {
                case '>': return (r.Rate > percentage) ? r : null;
                case '<': return (r.Rate < percentage) ? r : null;
                case '<=': return (r.Rate <= percentage) ? r : null;
                case '=>': return (r.Rate >= percentage) ? r : null;
                default: return r;
            }
        };

        rows = rows.filter(r => checkRow(r));

        return Object.assign({}, file, {Rows: rows});
    }

    setEditableSegment(segment, row) {
        return (e) => {
            const {recordDetails} = this.state;
            const {intl : { formatMessage }} = this.props;

            if (recordDetails && recordDetails.isFinalized) {
                toastr.removeByType('error');
                return toastr.error('', formatMessage(generalMessages.finalizedFileNotification));
            }

            if (segment.isLocked) {
                toastr.removeByType('error');
                return toastr.error('', formatMessage(generalMessages.lockedSegmentNotification));
            }

            let {editableText} = this.state;
            let currentValue = this.segmentNode && this.segmentNode.innerText;

            editableText = editableText && editableText.trim();
            currentValue = currentValue && currentValue.trim();

            if (editableText !== currentValue) {
                return this.setState({unsavedChanges: true})
            }

            e.target.focus();
            const cursorPosition = window.getSelection().getRangeAt(0).startOffset;

            this.setState({
                segmentBeforeEdit: segment,
                editableRowId: row.UnitId,
                editableSegmentId: segment.SegmentId,
                editableSubSegmentId: segment.SubSegmentId,
                editableText: segment.Text,
                cursorPosition
            })
        }
    }

    saveSegment(text) {
        return () => {
            const {editableRowId: RowId, editableSegmentId: SegmentId, editableSubSegmentId: SubSegmentId, editableText, recordDetails} = this.state;
            const {currentUser} = this.props;

            let value = null;

            // Needs checking for empty string (text !== '') for cleaning segment value
            if (!text && text !== '') value = this.segmentNode && this.segmentNode.innerHTML;

            if (editableText === value)
                return this.cancelSegmentEditing();

            const newText = text || value;

            const data = {
                FileId: recordDetails.FileId,
                RowId,
                SegmentId,
                SubSegmentId,
                ContactID: currentUser.ContactId,
                Text: newText.replace(/<br>/g, '')
            };

            this.updateRow(data, RowId);
        }
    }

    cancelSegmentEditing() {
        const {editableText} = this.state;

        if (this.segmentNode) this.segmentNode.innerHTML = editableText;

        this.setState({
            editableRowId: null,
            editableSegmentId: null,
            editableSubSegmentId: null,
            editableText: null,
            unsavedChanges: false
        })
    }

    toggleLog(row) {
        return () => {
            const {openedLogs} = this.state;

            if (openedLogs.includes(row.UnitId))
                openedLogs.splice(openedLogs.indexOf(row.UnitId), 1);
            else
                openedLogs.push(row.UnitId);

            this.setState({openedLogs});
        }
    }

    revertSegment(segment, row, isFirst) {
        return () => {
            const {RowId, SegmentId, SubSegmentId, OldText, NewText} = segment;
            const {FileId} = row;
            const {currentUser} = this.props;

            const newText = isFirst ? OldText : NewText;

            const data = {
                FileId,
                RowId,
                SegmentId,
                SubSegmentId,
                ContactID: currentUser.ContactId,
                Text: newText.replace(/<br>/g, '')
            };

            this.updateRow(data, RowId);
        }
    }

    updateRow(data, rowId) {
        this.setState({processSavingSegment: true});
        this.props.filesActions.updateSegment([data])
            .then(editedRow => {
                const {recordDetails} = this.state;
                let rows = recordDetails.Rows;

                rows = rows.map(r =>
                    r.UnitId === rowId
                        ? editedRow && editedRow[0]
                        : r
                );

                this.setState({
                    recordDetails: Object.assign({}, recordDetails, {Rows: rows}),
                    editableRowId: null,
                    editableSegmentId: null,
                    editableSubSegmentId: null,
                    editableText: null,
                    processSavingSegment: false,
                    unsavedChanges: false
                })
            })
            .catch(e => {
                this.setState({
                    processSavingSegment: false,
                    unsavedChanges: false,
                    editableRowId: null,
                    editableSegmentId: null,
                    editableSubSegmentId: null,
                    editableText: null
                });
                console.error(e);
            });
    }

    onModalReplace(recordDetails) {
        this.setState({recordDetails})
    }

    toggleKeyboard() {
        this.setState({
            openKeyboard: !this.state.openKeyboard
        })
    }

    closeKeyboard() {
        this.setState({openKeyboard: false})
    }

    changePdfSearchInOtherTab(text) {
        const container = this.popup.document.getElementById('outerContainer');

        if (!container) return setTimeout(this.changePdfSearchInOtherTab.bind(this), 500, text);

        const searchButton = container.querySelector('#viewFind');
        if (!searchButton) return;

        if (!searchButton.classList.contains('toggled')) {
            searchButton.click();
        }

        const input = container.querySelector('#findInput');
        if (!input) return;
        input.value = text && text.trim();

        const findNext = container.querySelector('#findNext');
        if (!findNext) return;
        findNext.click();

        const findHighlightAll = container.querySelector('#findHighlightAll');
        if (!findHighlightAll) return;
        if (!findHighlightAll.checked) findHighlightAll.click();
    }

    openPdfWindowTab(text) {
        return () => {
            const {pdfObjectURL} = this.state;
            if (!pdfObjectURL) return;

            if (this.popup && !this.popup.closed) {
                this.changePdfSearchInOtherTab(text);
                return this.popup.focus();
            }
            const isDev = process.env.NODE_ENV === 'development';
            this.popup = window.open(`${isDev ? '/pdf/web/viewer.html' : `/${this.props.locale}/validate/index/pdf`}?file=${encodeURI(pdfObjectURL)}`, '_blank');
            this.changePdfSearchInOtherTab(text);
        }
    }

    finalizeFile() {
        const { filesActions } = this.props;
        const { recordDetails: {FileId: fileId}, taskId } = this.state;
        this.setState({finalizeProcess:  true});
        filesActions.finalizeFile({taskId, fileId}).then(() => {
            const {recordDetails} = this.state;
            this.setState({
                recordDetails: {
                    ...recordDetails,
                    isFinalized: true
                }
            })
        }).catch(e => console.log(e))
            .finally(() => this.setState({finalizeProcess:  false}))
    }

    downloadPdf() {
        const {recordDetails: {FileId: fileId, PdfName}, taskId} = this.state;
        const {filesActions} = this.props;

        this.setState({loadingPdf: true});
        filesActions.downloadPdfReference({fileId, taskId})
            .then(res => FileDownload(res, PdfName))
            .catch(e => console.log(e))
            .finally(() => this.setState({loadingPdf: false}))
    }

    render() {
        const {openPanel, loadingDetails, itemForDisplay, loadingBookmarks, bookmarks, loadingComments,
            comments, editableRowId, editableSegmentId, editableSubSegmentId, openedLogs, hoveredSegment, hoveredRow,
            showDiff, onlyCorrection, finalizeProcess, loadingPdf} = this.state;
        let {recordDetails, currentNavigation, redirectToList} = this.state;
        const {matches, percentage, locale, intl: {formatMessage, formatNumber}} = this.props;

        if (redirectToList) return <Redirect to={`/${locale}/validate/index/list`} />;

        const selectedMatches = matches.filter(m => m.selected);
        const selectedMatchesValue = selectedMatches && selectedMatches[0] && selectedMatches[0].value;
        const selectedPercentage = percentage.filter(p => p.selected);
        const selectedPercentageValue = selectedPercentage && selectedPercentage[0] && selectedPercentage[0].value;

        if (recordDetails) recordDetails = this.applyMathFilter(recordDetails, {matched: selectedMatchesValue, percentage: +selectedPercentageValue});

        let rows = recordDetails ? recordDetails.Rows : [];
        if (onlyCorrection) rows = rows.filter(r => r.Logs);

        const paginationTotal = Math.ceil(rows.length / itemForDisplay);
        currentNavigation = currentNavigation > paginationTotal ? paginationTotal : currentNavigation;
        const slicedRows = rows.slice((currentNavigation - 1) * itemForDisplay, currentNavigation * itemForDisplay);

        return (
            <div className="tl-task-preview" ref={node => this.taskPreview = node}>
                <div className={`tl-task-preview-left ${openPanel ? '' : 'tl-closed-sidebar'}`}>
                    <div className="tl-left-fixed-panel">
                        <div className="tl-toggle-task-panel">
                            <button onClick={this.toggleSidebar}>
                                {openPanel ? '-' : '+'}
                            </button>
                        </div>
                        {openPanel && <div className="tl-task-left-panels">
                            <div className="tl-task-panel">
                                <div className="tl-task-panel-header">
                                    <FormattedMessage
                                        id="leftPanel.bookmarks"
                                        defaultMessage="Bookmarks"
                                    />
                                </div>
                                <div className="tl-task-panel-body">
                                    {loadingBookmarks && <div>
                                        <h3 style={{display: 'flex', alignItems: 'center', margin: '0'}}>
                                            <img
                                                src="/img/loading.gif"
                                                className="tl-loading-gif"
                                                alt={formatMessage(generalMessages.loading)}
                                            />
                                            {formatMessage(generalMessages.loading)}
                                        </h3>
                                    </div>}
                                    {!loadingBookmarks && bookmarks && bookmarks[0] && <ul className="tl-bookmarks-list">
                                        {bookmarks.map(bookmark => (
                                            <li key={bookmark.Id} onClick={this.scrollToUnit(bookmark)}>
                                                <BookmarkIcon
                                                    width={12}
                                                    height={12}
                                                />
                                                <span>{bookmark.Text && bookmark.Text.trim()}</span>
                                            </li>
                                        ))}
                                    </ul>}

                                    {!loadingBookmarks && (!bookmarks || !bookmarks[0]) && <FormattedMessage
                                        id="leftPanel.noBookmarks"
                                        defaultMessage="(no bookmarks)"
                                    />}

                                </div>
                            </div>

                            {recordDetails && recordDetails.PdfName && <div className="tl-task-panel">
                                <div className="tl-task-panel-header">
                                    <FormattedMessage
                                        id="leftPanel.references"
                                        defaultMessage="References"
                                    />
                                </div>
                                <div className="tl-task-panel-body">
                                    <div
                                        onClick={this.downloadPdf}
                                        className="tl-download-pdf-link"
                                    >
                                        {recordDetails.PdfName}

                                        {loadingPdf && <img
                                            src="/img/loading.gif"
                                            className="tl-loading-gif"
                                            alt={formatMessage(generalMessages.loading)}
                                        />}
                                    </div>
                                </div>
                            </div>}

                            <div className="tl-task-panel">
                                <div className="tl-task-panel-header">
                                    <FormattedMessage
                                        id="leftPanel.comments"
                                        defaultMessage="Comments"
                                    />
                                    <button className="tl-task-add-new-comment" onClick={this.showModal('openCommentsModal')}>+</button>
                                </div>
                                <div className="tl-task-panel-body">
                                    {loadingComments && <div>
                                        <h3 style={{display: 'flex', alignItems: 'center', margin: '0'}}>
                                            <img
                                                src="/img/loading.gif"
                                                className="tl-loading-gif"
                                                alt={formatMessage(generalMessages.loading)}
                                            />
                                            {formatMessage(generalMessages.loading)}
                                        </h3>
                                    </div>}
                                    {!loadingComments && comments && comments[0] && <ul className="tl-comments-list">
                                        {comments.map(comment => (
                                            <li key={comment.Id}>
                                                <span className="tl-comment-name">
                                                    <CommentIcon
                                                        width={12}
                                                        height={12}
                                                        fill={'#c50008'}
                                                    />
                                                    <span>{comment.Creator && comment.Creator.Name}</span>
                                                    {comment.RowId && <span className="tl-link-to-comment" onClick={this.scrollToUnit(comment)}>
                                                        <CommentArrowIcon
                                                            width={12}
                                                            height={12}
                                                        />
                                                    </span>}
                                                </span>
                                                <span>{comment.Text}</span>
                                            </li>
                                        ))}
                                    </ul>}

                                    {!loadingComments && (!comments || !comments[0]) && <FormattedMessage
                                        id="leftPanel.noComments"
                                        defaultMessage="(no comments)"
                                    />}

                                </div>
                            </div>
                        </div>}
                    </div>
                </div>
                <div className="tl-task-preview-right">
                    <div className="tl-task-top-panel" ref={panelNode => this.panelNode = panelNode}>
                        <div className="tl-back-to-projects">
                            <Link to={`/${locale}/validate/index/list`}>
                                <FormattedMessage
                                    id="recordView.backToTasksButton"
                                    defaultMessage="Back to the list"
                                />
                            </Link>
                        </div>
                        <div className="tl-task-search-panel">
                            <button className="tl-primary-button" onClick={this.showModal('openSearchModal')} disabled={!recordDetails}>
                                <SearchIcon
                                    width={12}
                                    height={12}
                                />
                                <FormattedMessage
                                    id="recordSearchPanel.searchButton"
                                    defaultMessage="Search"
                                />
                            </button>
                            <button className="tl-blue-button" onClick={() => this.setState({onlyCorrection: !onlyCorrection})}>
                                <WarningIcon
                                    width={12}
                                    height={12}
                                />

                                {onlyCorrection
                                    ? <FormattedMessage
                                        id="recordSearchPanel.showAllButton"
                                        defaultMessage="Show all"
                                    />
                                    : <FormattedMessage
                                        id="recordSearchPanel.showCorrectionsButton"
                                        defaultMessage="Only show corrections"
                                    />
                                }
                            </button>
                            <button className="tl-blue-button" onClick={() => this.setState({showDiff: !showDiff})}>
                                <DiffIcon
                                    width={12}
                                    height={12}
                                />
                                {showDiff
                                    ? <FormattedMessage
                                        id="recordSearchPanel.hideDiffButton"
                                        defaultMessage="Hide diff"
                                    />
                                    : <FormattedMessage
                                        id="recordSearchPanel.showDiffButton"
                                        defaultMessage="Show diff"
                                    />
                                }
                            </button>
                            {recordDetails && !recordDetails.isFinalized && <button
                                className="tl-green-button"
                                onClick={this.finalizeFile}
                                disabled={finalizeProcess}
                            >
                                <FormattedMessage
                                    id="recordSearchPanel.finalizeFileLabel"
                                    defaultMessage="Finalize"
                                />
                            </button>}
                            {recordDetails && recordDetails.isFinalized && <div className="tl-finalized-task">
                                <FormattedMessage
                                    id="recordSearchPanel.alreadyFinalizeFileLabel"
                                    defaultMessage="File is finalized!"
                                />
                            </div>}
                            {finalizeProcess && <div className="tl-files-table-row" style={{margin: '4px 0 4px 10px'}}>
                                <img src="/img/loading.gif" className="tl-loading-gif" alt={formatMessage(generalMessages.loading)} style={{marginRight: 0}} />
                            </div>}
                            <div className="tl-task-search-right">
                                <FormattedMessage
                                    id="recordSearchPanel.showMatchesLabel"
                                    defaultMessage="Show matches"
                                />
                                <select
                                    defaultValue={selectedMatchesValue}
                                    onChange={this.onSelectChange('matches')}
                                >
                                    {matches.map((m, index) => (
                                        <option key={index} value={m.value}>{m.name}</option>
                                    ))}
                                </select>
                                <FormattedMessage
                                    id="recordSearchPanel.showPercentageLabel"
                                    defaultMessage="than"
                                />
                                <select
                                    defaultValue={selectedPercentageValue}
                                    onChange={this.onSelectChange('percentage')}
                                >
                                    {percentage.map((m, index) => (
                                        <option key={index} value={m.value}>
                                            {+m.name
                                                ? formatNumber(m.name, {style: 'percent'})
                                                : formatMessage(messages.noMatchSelectOption)
                                            }
                                        </option>
                                    ))}
                                </select>
                            </div>
                        </div>

                        {loadingDetails && <div>
                            <h3 style={{display: 'flex', alignItems: 'center'}}>
                                <img
                                    src="/img/loading.gif"
                                    className="tl-loading-gif"
                                    alt={formatMessage(generalMessages.loading)}
                                />
                                <FormattedMessage
                                    id="recordSearchPanel.loadingRecordDetails"
                                    defaultMessage="Loading details..."
                                />
                            </h3>
                        </div>}

                        {!loadingDetails && recordDetails && <div>
                            <div className="tl-task-table-row tl-task-table-header-row">
                                <div className="tl-task-table-col">{recordDetails.SourceLanguage}</div>
                                <div className="tl-task-table-col">{recordDetails.TargetLanguage}</div>
                                <div className="tl-task-table-col tl-task-table-col-match">
                                    <FormattedMessage
                                        id="recordSearchPanel.matchColumnTitle"
                                        defaultMessage="match"
                                    />
                                </div>
                            </div>

                            <div className="tl-task-table-row tl-task-table-sub-header-row">
                                <div className="tl-task-table-col">{recordDetails.FileName}</div>
                                <div className="tl-task-table-col tl-task-table-col-page">
                                    <FormattedMessage
                                        id="recordSearchPanel.filePaginationTitle"
                                        defaultMessage="pg {current} / {total}"
                                        values={{current: currentNavigation, total: paginationTotal}}
                                        tagName="em"
                                    />
                                </div>
                            </div>
                        </div>}

                    </div>

                    <div className="tl-task-rows-wrapper" ref={taskContainerNode => this.taskContainerNode = taskContainerNode}>
                        {!loadingDetails && recordDetails && <div>

                            {slicedRows && slicedRows[0] && slicedRows.map((row, index) => {
                                const isRowEditable = editableRowId === row.UnitId;
                                const isOpenLog = openedLogs.includes(row.UnitId);

                                return (
                                    <div key={index} className="tl-task-table-row">
                                        <div className="tl-task-table-col">
                                            {row.SourceSegments && row.SourceSegments[0] && row.SourceSegments.map((item, index) => (
                                                <span key={index}>
                                                    <span
                                                        onClick={recordDetails.PdfName ? this.openPdfWindowTab(item.Text) : null}
                                                        className={recordDetails.PdfName ? 'tl-segment-reference' : ''}
                                                    >
                                                        {item.Text}
                                                    </span>
                                                    {item.isEndOfLine && <br />}
                                                </span>
                                            ))}
                                        </div>
                                        <div className="tl-task-table-col tl-target-col" id={`tl-unit-${row.UnitId}`}>
                                            <span
                                                className={`tl-task-bookmark ${row.BookmarkId ? 'tl-active-bookmark' : ''}`}
                                                onClick={this.addBookmark(row)}
                                            ></span>
                                            {!isRowEditable && <div
                                                className={`tl-task-comment ${row.CommentsCount ? 'tl-task-with-comments' : ''}`}
                                                onClick={this.showModal('openCommentsModal', row)}
                                            >
                                                <CommentIcon
                                                    width={12}
                                                    height={12}
                                                    fill={'#fff'}
                                                />
                                                {(row.CommentsCount > 0) && <span>{row.CommentsCount}</span>}
                                            </div>}

                                            {this.state.unsavedChanges && isRowEditable && <span className="tl-unsaved-changes">
                                                <span className="tl-unsaved-bg">
                                                    <FormattedMessage
                                                        id="recordView.unsavedChangesHint"
                                                        defaultMessage='Unsaved {br} changes'
                                                        values={{
                                                            br: <br />
                                                        }}
                                                    />
                                                </span>
                                            </span>}

                                            {row.TargetSegments && row.TargetSegments[0] && row.TargetSegments.map((item, index) => {
                                                const isEditable = (editableRowId === row.UnitId) && (editableSegmentId === item.SegmentId) && (editableSubSegmentId === item.SubSegmentId);

                                                return (
                                                    (showDiff && (item.OldValue || item.OldValue === '') && !isEditable) // Needs additional check for correct diff into empty segments
                                                        ? <span key={index}>
                                                            <span onClick={this.setEditableSegment(item, row)}>
                                                                <ShowSegmentDiff
                                                                    oldValue={item.OldValue}
                                                                    newValue={item.Text}
                                                                />
                                                            </span>
                                                            {item.isEndOfLine && <br />}
                                                        </span>
                                                        : <span key={index}>
                                                            <span
                                                                onClick={this.setEditableSegment(item, row)}
                                                                className={`tl-content-editable-container tl-editable-span ${isEditable ? 'tl-editable-segment' : ''} ${item.OldValue ? 'tl-segment-with-log' : ''} ${(hoveredRow === row.UnitId && hoveredSegment === item.SegmentId) ? 'tl-hovered-segment' : ''} ${item.isLocked ? '' : 'tl-pointer-segment'}`}
                                                                ref={isEditable ? segmentNode => this.segmentNode = segmentNode : null}
                                                                contentEditable={isEditable}
                                                                spellCheck={false}
                                                                suppressContentEditableWarning={true}
                                                            >
                                                                {item.Text}
                                                            </span>
                                                            {item.isEndOfLine && <br />}
                                                        </span>
                                                )
                                            })}

                                            {isRowEditable && <div className="tl-editable-row-form">
                                                <div className={`tl-task-comment ${row.CommentsCount ? 'tl-task-with-comments' : ''}`} onClick={this.showModal('openCommentsModal', row)}>
                                                    <CommentIcon
                                                        width={12}
                                                        height={12}
                                                        fill={'#fff'}
                                                    />
                                                    {(row.CommentsCount > 0) && <span>{row.CommentsCount}</span>}
                                                </div>
                                                <div className="tl-segments-actions">
                                                    {this.state.processSavingSegment && <img
                                                        src="/img/loading.gif"
                                                        className="tl-loading-gif"
                                                        alt={formatMessage(generalMessages.loading)}
                                                    />}
                                                    <button
                                                        className="tl-segment-save"
                                                        onClick={this.saveSegment(null)}
                                                        disabled={this.state.processSavingSegment}
                                                    >
                                                        {formatMessage(generalMessages.saveButton)}
                                                    </button>
                                                    <button className="tl-segment-cancel" onClick={this.cancelSegmentEditing}>
                                                        {formatMessage(generalMessages.cancelButton)}
                                                    </button>
                                                    <button className="tl-keyboard-toggle" onClick={this.toggleKeyboard}>
                                                        <KeyboardIcon
                                                            width={16}
                                                            height={16}
                                                        />
                                                    </button>
                                                    {this.state.openKeyboard && <TLKeyboard
                                                        editableText={this.state.editableText}
                                                        closeKeyboard={this.closeKeyboard}
                                                        saveSegment={this.saveSegment}
                                                        cursorPosition={this.state.cursorPosition}
                                                    />}
                                                </div>
                                            </div>}

                                            {row.Logs && row.Logs[0] && <div>
                                                <div className="tl-log-toggle-button" onClick={this.toggleLog(row)}>
                                                    <ToggleDetailsIcon
                                                        width={24}
                                                        height={24}
                                                        isOpenDetails={isOpenLog}
                                                    />
                                                    <FormattedMessage
                                                        id="recordView.openLogHint"
                                                        defaultMessage="Log"
                                                    />
                                                </div>

                                                <div className={`tl-log-details ${isOpenLog ? 'tl-opened-log' : ''}`}>
                                                    {row.Logs.map((logGroup,index) => (
                                                        <div key={index}>
                                                            <div
                                                                className="tl-log-group-first"
                                                            >
                                                                {logGroup[0] && <div>{logGroup[0].OldText}</div>}
                                                                {logGroup[0] && <div className="tl-revert-segment">
                                                                    <img
                                                                        src="/img/history-revert.png"
                                                                        alt={formatMessage(generalMessages.revertHint)}
                                                                        onClick={this.revertSegment(logGroup[0], row, true)}
                                                                    />
                                                                </div>}
                                                            </div>
                                                            <div className="tl-log-group-details">
                                                                {logGroup.map((log, index) => {
                                                                    const lastItem = logGroup.length === index+1;
                                                                    return (
                                                                        <div
                                                                            key={index}
                                                                            className="tl-clearfix tl-log-group-row"
                                                                        >
                                                                            <div>
                                                                                <span className="tl-segment-sign">{lastItem ? '=>' : '---'}</span>
                                                                                <span className="tl-segment-author">{log.ModifiedBy && log.ModifiedBy.Name}</span>
                                                                                <span
                                                                                    className={lastItem ? 'tl-last-segment-row' : ''}
                                                                                >
                                                                                    {log.NewText}
                                                                                </span>
                                                                            </div>

                                                                            {!lastItem && <div className="tl-revert-segment">
                                                                                <img
                                                                                    src="/img/history-revert.png"
                                                                                    alt={formatMessage(generalMessages.revertHint)}
                                                                                    onClick={this.revertSegment(log, row, false)}
                                                                                />
                                                                            </div>}
                                                                        </div>
                                                                    )
                                                                })}
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>

                                            </div>}

                                        </div>

                                        <div className="tl-task-table-col tl-task-table-col-match tl-no-match-style">
                                            {row.Rate > 50
                                                ? formatNumber(row.Rate / 100, {style: 'percent'})
                                                : formatMessage(messages.noMatch)
                                            }
                                        </div>
                                    </div>
                                )
                            })}

                            {(currentNavigation === paginationTotal) && <div className="tl-task-table-row tl-task-table-end-file">
                                <div className="tl-task-table-col">
                                    <FormattedMessage
                                        id="recordView.endFileMessage"
                                        defaultMessage="You have reached the end of the file"
                                    />
                                </div>
                            </div>}

                            <div className="tl-text-center">
                                <ReactPaginate
                                    previousLabel={formatMessage(generalMessages.paginationPrevHint)}
                                    nextLabel={formatMessage(generalMessages.paginationNextHint)}
                                    breakLabel={<a href="" onClick={e => e.preventDefault()}>...</a>}
                                    breakClassName={"tl-break-navigation"}
                                    pageCount={paginationTotal}
                                    marginPagesDisplayed={3}
                                    pageRangeDisplayed={5}
                                    onPageChange={this.handlePageClick}
                                    containerClassName={"tl-list-paginate"}
                                    subContainerClassName={"pages pagination"}
                                    activeClassName={"tl-active-navigation"}
                                />
                            </div>

                        </div>}
                    </div>

                </div>

                {this.state.openSearchModal && <SearchRecordsModal
                    closeModal={this.closeModal}
                    recordDetails={recordDetails}
                    filesActions={this.props.filesActions}
                    currentUser={this.props.currentUser}
                    onReplace={this.onModalReplace}
                />}

                {this.state.openCommentsModal && <CommentsModal
                    closeModal={this.closeModal}
                    rowForComment={this.state.rowForComment}
                    targetFileId={this.state.recordDetails && this.state.recordDetails.FileId}
                    onAddComment={this.onAddComment}
                    allComments={comments}
                />}

            </div>
        );
    }
}

RecordView.propTypes = {
    filesActions: PropTypes.object.isRequired,
    configActions: PropTypes.object.isRequired,
    currentUser: PropTypes.object.isRequired,
    matches: PropTypes.array.isRequired,
    percentage: PropTypes.array.isRequired,
    locale: PropTypes.string.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        currentUser: state.currentUser,
        matches: state.config.matches,
        percentage: state.config.percentage,
        locale: state.config.locale
    }
}

function mapDispatchToProps(dispatch) {
    return {
        filesActions: bindActionCreators(filesActions, dispatch),
        configActions: bindActionCreators(configActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(RecordView))
