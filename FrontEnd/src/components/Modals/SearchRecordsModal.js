import React, { Component } from 'react'
import Modal from 'react-modal'
import PropTypes from 'prop-types'
import ReactPaginate from 'react-paginate'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'

// Parts
import modalStyles from './modalStyles'
import generalMessages from '../../i18n/generalMessages'

const messages = defineMessages({
    enterSearchValue: {
        id: 'searchModal.enterSearchValue',
        defaultMessage: 'Enter search value'
    },
    enterReplaceValue: {
        id: 'searchModal.enterReplaceValue',
        defaultMessage: 'Enter replace value'
    }
});

class SearchRecordsModal extends Component {
    constructor(props){
        super(props);
        this.state = {
            currentNavigation: 1,
            itemForDisplay: 100,
            searchField: '',
            replaceField: '',
            shouldReplace: false,
            selectedRowsForReplace: []
        };

        this.handlePageClick = this.handlePageClick.bind(this);
        this.changeField = this.changeField.bind(this);
        this.applyReplace = this.applyReplace.bind(this);
        this.replaceSegments = this.replaceSegments.bind(this);
        this.changeSelectedRowsForReplace = this.changeSelectedRowsForReplace.bind(this);
        this.onBlurEditable = this.onBlurEditable.bind(this);
    }

    componentDidMount() {
        this.setState({recordDetails: JSON.parse(JSON.stringify(this.props.recordDetails))});
        document.addEventListener('click', this.onBlurEditable)
    }

    componentWillUnmount() {
        document.removeEventListener('click', this.onBlurEditable)
    }

    onBlurEditable(e, callback) {
        const {showReplaced, editableRowId, editableSegmentId, recordDetails} = this.state;

        if (!showReplaced || !editableRowId || !editableSegmentId || (e && e.target && e.target.closest('.tl-task-replaced-col')) ) return;

        const editedValue = this.segmentNode.innerHTML;
        const rows = recordDetails.Rows;

        const editedRows = rows.map(r =>
            r.UnitId === editableRowId
                ? Object.assign({}, r, {replacedSegments: r.replacedSegments.map(s =>
                    s.SegmentId === editableSegmentId
                        ? Object.assign({}, s, {replaced: editedValue})
                        : s
                )})
                : r
            );

        if (callback) {
            return this.setState({
                recordDetails: Object.assign({}, recordDetails, {Rows: editedRows})
            }, callback);
        }

        this.setState({
            recordDetails: Object.assign({}, recordDetails, {Rows: editedRows}),
            editableRowId: null,
            editableSegmentId: null
        });
    }

    handlePageClick(data) {
        this.setState({currentNavigation: data.selected + 1});
    };

    changeField(type) {
        return e => {
            if (type === 'shouldReplace') return this.setState({shouldReplace: e.target.checked, showReplaced: false});
            if (type === 'searchField') this.setState({showReplaced: false});
            this.setState({[type]: e.target.value, previewError: null, updateSuccess: null})
        }
    }

    filterRows(rows, text) {
        if (!text) return rows;

        const clearHtml = html => {
            const tmp = document.createElement('DIV');
            tmp.innerHTML = html;
            return tmp.textContent || tmp.innerText || '';
        };

        const req = new RegExp(text, 'gi');

        return rows.filter(row => {
            const sourceSegments = row.SourceSegments || [];
            const targetSegments = row.TargetSegments || [];
            let isMatch = false;

            for (let i = 0; i < sourceSegments.length; i++) {
                const clearedText = clearHtml(sourceSegments[i].Text);
                if (clearedText.toLowerCase().indexOf(text.toLowerCase()) !== -1) isMatch = true;
                sourceSegments[i].markedText = clearedText.replace(req, match => `<span class="tl-highlighted-text">${match}</span>`);
            }

            for (let i = 0; i < targetSegments.length; i++) {
                const clearedText = clearHtml(targetSegments[i].Text);
                if (clearedText.toLowerCase().indexOf(text.toLowerCase()) !== -1) isMatch = true;
                targetSegments[i].markedText = clearedText.replace(req, match => `<span class="tl-highlighted-text">${match}</span>`);
            }

            return isMatch ? row : null
        });
    }

    setEditableSegment(segment, row) {
        return () => {
            const {editableRowId, editableSegmentId} = this.state;

            if (editableSegmentId || editableRowId) {
                this.onBlurEditable(null, () => {
                    this.setState({
                        editableRowId: row.UnitId,
                        editableSegmentId: segment.SegmentId
                    })
                });
            } else {
                this.setState({
                    editableRowId: row.UnitId,
                    editableSegmentId: segment.SegmentId
                })
            }
        }
    }

    applyReplace() {
        const {intl: {formatMessage}} = this.props;
        const {searchField, replaceField} = this.state;
        let {recordDetails} = this.state;

        if (!searchField) return this.setState({previewError: formatMessage(messages.enterSearchValue)});
        if (!replaceField) return this.setState({previewError: formatMessage(messages.enterReplaceValue)});

        const replacedReq = new RegExp(searchField, 'gi');

        let rows = recordDetails.Rows;

        rows = rows.map(r => {
            const replacedSegments = r.TargetSegments ? r.TargetSegments.map(s => {
                const replaced = s.Text.replace(replacedReq, replaceField);
                return Object.assign({}, s, {replaced});
            }) : [];
            return Object.assign({}, r, {replacedSegments});
        });

        recordDetails = Object.assign({}, recordDetails, {Rows: rows});

        this.setState({showReplaced: true, recordDetails, selectedRowsForReplace: []})
    }

    replaceSegments() {
        const {recordDetails, selectedRowsForReplace} = this.state;
        const {currentUser} = this.props;

        const rows = recordDetails.Rows;

        const filteredRows = rows.filter(r => {
            if (!selectedRowsForReplace.includes(r.UnitId)) return false;
            let isChangedSegment = false;
            r.replacedSegments.forEach(s => {
                if (s.replaced !== s.Text) isChangedSegment = true;
            });
            return isChangedSegment;
        });

        const data = [];

        filteredRows.forEach(r => {
            r.replacedSegments.forEach(s => {
                if (s.replaced === s.Text) return;
                data.push({
                    FileId: r.FileId,
                    RowId: r.UnitId,
                    SegmentId: s.SegmentId,
                    ContactID: currentUser.ContactId,
                    Text: s.replaced
                });
            });
        });

        if(!data.length) return;

        this.setState({updateProcessing: true});

        this.props.filesActions.updateSegment(data)
            .then(editedRows => {
                const {recordDetails} = this.state;
                const rows = recordDetails.Rows;
                const newRows = rows.map(row => {
                    const foundedRow = editedRows.find(r => r.UnitId === row.UnitId);
                    return foundedRow || row
                });

                const newRecord = Object.assign({}, recordDetails, {Rows: newRows});

                this.setState({
                    recordDetails: newRecord,
                    searchField: '',
                    replaceField: '',
                    showReplaced: false,
                    selectedRowsForReplace: []
                });

                this.props.onReplace(newRecord);

                this.setState({updateProcessing: false, updateSuccess: true});
            })
            .catch(e => console.error(e));
    }

    changeSelectedRowsForReplace(id) {
        return e => {
            if (id === 'all') {
                if (!e.target.checked) return this.setState({selectedRowsForReplace: []});

                const {searchField, recordDetails} = this.state;
                let rows = recordDetails.Rows;
                rows = searchField.trim() ? this.filterRows(rows, searchField.trim()) : rows;
                const selectedRowsForReplace = rows.map(r => r.UnitId);

                return this.setState({selectedRowsForReplace})
            } else {
                const {selectedRowsForReplace} = this.state;
                if (selectedRowsForReplace.includes(id))
                    selectedRowsForReplace.splice(selectedRowsForReplace.indexOf(id), 1);
                else
                    selectedRowsForReplace.push(id);

                return this.setState({selectedRowsForReplace})
            }
        }
    }

    render() {
        const {closeModal, intl: {formatMessage}} = this.props;
        let {currentNavigation, itemForDisplay, searchField, replaceField, shouldReplace, editableSegmentId,
            editableRowId, showReplaced, recordDetails, selectedRowsForReplace,
            previewError} = this.state;
        const modalStyle = window.innerWidth < 768 ? modalStyles.mobile : modalStyles.desktop;

        let rows = recordDetails ? recordDetails.Rows : [];
        rows = searchField.trim() ? this.filterRows(rows, searchField.trim()) : rows;

        const paginationTotal = Math.ceil(rows.length / itemForDisplay);
        currentNavigation = currentNavigation > paginationTotal ? paginationTotal : currentNavigation;
        const slicedRows = rows.slice((currentNavigation - 1) * itemForDisplay, currentNavigation * itemForDisplay);

        return (
            <Modal
                isOpen={true}
                contentLabel="tl-modal"
                style={modalStyle}
            >
                <div className="tl-modal-container">
                    <div className="tl-modal-header">
                        <FormattedMessage
                            id="searchModal.modalTitle"
                            defaultMessage="Search"
                            tagName="h4"
                        />
                        <button onClick={closeModal}>×</button>
                    </div>
                    <div className="tl-modal-body">

                        <div className="tl-search-segments-row">
                            <FormattedMessage
                                id="searchModal.searchLabel"
                                defaultMessage="Search"
                                tagName="strong"
                            />
                            <input
                                type="text"
                                className="tl-search-segment-field"
                                value={searchField}
                                onChange={this.changeField('searchField')}
                            />
                        </div>

                        <div className="tl-search-segments-row tl-replace-checkbox-row">
                            <FormattedMessage
                                id="searchModal.replaceAndSearchLabel"
                                defaultMessage="Search and Replace"
                                tagName="strong"
                            />
                            <input
                                type="checkbox"
                                onChange={this.changeField('shouldReplace')}
                                checked={shouldReplace}
                            />
                        </div>

                        {shouldReplace && <div className="tl-search-segments-row">
                            <FormattedMessage
                                id="searchModal.replaceLabel"
                                defaultMessage="Replace"
                                tagName="strong"
                            />
                            <input
                                type="text"
                                className="tl-search-segment-field"
                                value={replaceField}
                                onChange={this.changeField('replaceField')}
                            />
                            <button className="tl-primary-button" onClick={this.applyReplace}>
                                <FormattedMessage
                                    id="searchModal.previewReplaceButton"
                                    defaultMessage="Preview Replace"
                                />
                            </button>
                            {previewError && <span className="tl-preview-error">{previewError}</span>}
                        </div>}

                        <div style={{display: 'flex', alignItems: 'center'}}>
                            <div className="tl-search-result">
                                <FormattedMessage
                                    id="searchModal.searchResultTitle"
                                    defaultMessage="Search returned {count}"
                                    values={{count: rows.length}}
                                />
                            </div>
                            <div className="tl-search-paginate">
                                {this.state.updateSuccess && <span className="tl-success-message">
                                    <FormattedMessage
                                        id="searchModal.updatedSuccessMessage"
                                        defaultMessage="The selected segments have been updated"
                                    />
                                </span>}
                                {this.state.updateProcessing && <img
                                    src="/img/loading.gif"
                                    className="tl-loading-gif"
                                    alt={formatMessage(generalMessages.loading)}
                                />}
                                {!!selectedRowsForReplace.length && <button
                                    className="tl-button tl-green-button"
                                    onClick={this.replaceSegments}
                                >
                                    <FormattedMessage
                                        id="searchModal.replaceButton"
                                        defaultMessage="Replace"
                                    />
                                </button>}
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
                        </div>

                        <div className="tl-task-table-row tl-task-table-header-row">
                            <div className="tl-task-table-col">{recordDetails && recordDetails.SourceLanguage}</div>
                            <div className="tl-task-table-col">{recordDetails && recordDetails.TargetLanguage}</div>
                            {showReplaced && <div className="tl-task-table-col">
                                <FormattedMessage
                                    id="searchModal.replacedColHeader"
                                    defaultMessage="Replaced"
                                />
                            </div>}
                            {showReplaced && <div className="tl-task-table-col tl-accept-editable">
                                <label htmlFor="checkAll">All</label>
                                <input
                                    id="checkAll"
                                    type="checkbox"
                                    onChange={this.changeSelectedRowsForReplace('all')}
                                />
                            </div>}
                        </div>

                        <div className={`tl-search-result-wrap ${searchField ? '' : 'tl-disable-highlighting'}`}>
                            {slicedRows && slicedRows[0] && slicedRows.map((row, index) => (
                                <div key={index} className="tl-task-table-row">

                                    <div className="tl-task-table-col">
                                        {row.SourceSegments && row.SourceSegments[0] && row.SourceSegments.map((item, index) => (
                                            <span
                                                key={index}
                                                dangerouslySetInnerHTML={{__html: item.markedText || item.Text}}
                                            ></span>
                                        ))}
                                    </div>
                                    <div className="tl-task-table-col">
                                        {row.TargetSegments && row.TargetSegments[0] && row.TargetSegments.map((item, index) => (
                                            <span
                                                key={index}
                                                dangerouslySetInnerHTML={{__html: item.markedText || item.Text}}
                                            ></span>
                                        ))}
                                    </div>

                                    {showReplaced && <div className="tl-task-table-col tl-task-replaced-col">
                                        {row.replacedSegments && row.replacedSegments[0] && row.replacedSegments.map((item, index) => {
                                            const isEditable = editableRowId === row.UnitId && editableSegmentId === item.SegmentId;

                                            return (
                                                <span
                                                    key={index}
                                                    className={`tl-editable-span ${isEditable ? 'tl-editable-segment' : ''}`}
                                                    contentEditable={isEditable}
                                                    spellCheck={false}
                                                    onClick={this.setEditableSegment(item, row)}
                                                    ref={isEditable ? node => this.segmentNode = node : null}
                                                    suppressContentEditableWarning={true}
                                                >
                                                    {item.replaced}
                                                </span>
                                            )
                                        })}
                                    </div>}

                                    {showReplaced && <div className="tl-task-table-col tl-accept-editable">
                                        <input
                                            type="checkbox"
                                            onChange={this.changeSelectedRowsForReplace(row.UnitId)}
                                            checked={selectedRowsForReplace.includes(row.UnitId)}
                                        />
                                    </div>}

                                </div>
                            ))}
                        </div>



                    </div>
                </div>


            </Modal>
        );
    }
}

SearchRecordsModal.propTypes = {
    closeModal: PropTypes.func.isRequired,
    recordDetails: PropTypes.object.isRequired,
    currentUser: PropTypes.object.isRequired,
    filesActions: PropTypes.object.isRequired,
    intl: intlShape.isRequired
};

export default injectIntl(SearchRecordsModal)