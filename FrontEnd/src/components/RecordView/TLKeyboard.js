import React, {Component} from 'react'
import PropTypes from 'prop-types'
import OkIcon from '../svgIcons/OkIcon'
import CloseIcon from '../svgIcons/CloseIcon'


class TLKeyboard extends Component {
    constructor(props) {
        super(props);
        this.state = {
            buttonsList: [
                '&', '"', '¢', '€', '£', '¥', '©', '®', '™', '‰', 'µ', '·', '•', '…', '′', '″', '§', '¶', 'ß', '‹', '›',
                '«', '»', '‘', '’', '“', '”', '‚', '„', '<', '>', '≤', '≥', '–', '—', '¯', '‾', '¤', '¦', '¨', '¡', '¿',
                'ˆ', '˜', '°', '−', '±', '÷', '⁄', '×', '¹', '²', '³', '¼', '½', '¾', 'ƒ', '∫', '∑', '∞', '√', '∼', '≅',
                '≈', '≠', '≡', '∈', '∉', '∋', '∏', '∧', '∨', '¬', '∩', '∪', '∂', '∀', '∃', '∅', '∇', '∗', '∝', '∠', '´',
                '¸', 'ª', 'º', '†', '‡', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ', 'Ç', 'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï',
                'Ð', 'Ñ', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', 'Ø', 'Œ', 'Š', 'Ù', 'Ú', 'Û', 'Ü', 'Ý', 'Ÿ', 'Þ', 'à', 'á', 'â', 'ã',
                'ä', 'å', 'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï', 'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö', 'ø', 'œ',
                'š', 'ù', 'ú', 'û', 'ü', 'ý', 'þ', 'ÿ', 'Α', 'Β', 'Γ', 'Δ', 'Ε', 'Ζ', 'Η', 'Θ', 'Ι', 'Κ', 'Λ', 'Μ', 'Ν',
                'Ξ', 'Ο', 'Π', 'Ρ', 'Σ', 'Τ', 'Υ', 'Φ', 'Χ', 'Ψ', 'Ω', 'α', 'β', 'γ', 'δ', 'ε', 'ζ', 'η', 'θ', 'ι', 'κ',
                'λ', 'μ', 'ν', 'ξ', 'ο', 'π', 'ρ', 'ς', 'σ', 'τ', 'υ', 'φ', 'χ', 'ψ', 'ω', 'ℵ', 'ϖ', 'ℜ', 'ϑ', 'ϒ', '℘',
                'ℑ', '←', '↑', '→', '↓', '↔', '↵', '⇐', '⇑', '⇒', '⇓', '⇔', '∴', '⊂', '⊃', '⊄', '⊆', '⊇', '⊕', '⊗', '⊥',
                '⋅', '◊', '♠', '♣', '♥', '♦'
            ]
        };

        this.onInputKeyDown = this.onInputKeyDown.bind(this);
        this.onInputChange = this.onInputChange.bind(this);
        this.addSymbol = this.addSymbol.bind(this);
        this.saveSegment = this.saveSegment.bind(this);
        this.onClickOutside = this.onClickOutside.bind(this);
    }

    componentDidMount() {
        const {editableText, cursorPosition} = this.props;

        const clearHtml = html => {
            const tmp = document.createElement('DIV');
            tmp.innerHTML = html;
            return tmp.textContent || tmp.innerText || '';
        };

        this.setState({
            editableText: clearHtml(editableText),
            cursorPosition
        }, () => {
            this.inputField.setSelectionRange(cursorPosition, cursorPosition);
            this.inputField.focus();
        });

        document.addEventListener('mousedown', this.onClickOutside)
    }

    componentWillUnmount() {
        document.removeEventListener('mousedown', this.onClickOutside)
    }

    onClickOutside(e) {
        if (this.keyboardWrap && !this.keyboardWrap.contains(e.target)) {
            this.props.closeKeyboard()
        }
    }

    onInputKeyDown(e) {
        this.setState({cursorPosition: e.target.selectionStart})
    }

    onInputChange(e) {
        this.setState({
            editableText: e.target.value,
            cursorPosition: e.target.selectionStart
        })
    }

    addSymbol(e) {
        const {cursorPosition, editableText} = this.state;

        const value = e.target.innerText;
        const newText = `${editableText.slice(0, cursorPosition)}${value}${editableText.slice(cursorPosition)}`;

        this.setState({editableText: newText, cursorPosition: cursorPosition + 1}, () => {
            this.inputField.setSelectionRange(cursorPosition + 1, cursorPosition + 1);
            this.inputField.focus()
        })
    }

    saveSegment() {
        const {editableText} = this.state;
        const {editableText: prevText, closeKeyboard, saveSegment} = this.props;

        if (editableText === prevText) return closeKeyboard();

        saveSegment(editableText).call();
        closeKeyboard();
    }

    render() {
        const {editableText, buttonsList} = this.state;

        return (
            <div className="tl-keyboard-wrap" ref={node => this.keyboardWrap = node}>
                <input
                    type="text"
                    value={editableText || ''}
                    onClick={this.onInputKeyDown}
                    onKeyDown={this.onInputKeyDown}
                    onChange={this.onInputChange}
                    ref={node => this.inputField = node}
                    spellCheck={false}
                />
                <div className="tl-keyboard-buttons">
                    {buttonsList.map((value, index) => (
                        <button key={index} onClick={this.addSymbol}>{value}</button>
                    ))}
                </div>
                <div className="tl-keyboard-actions">
                    <button className="tl-editable-ok" onClick={this.saveSegment}>
                        <OkIcon width={18} height={18} />
                    </button>
                    <button className="tl-editable-cancel" onClick={() => this.props.closeKeyboard()}>
                        <CloseIcon width={18} height={18} />
                    </button>
                </div>
            </div>
        )
    }

}

TLKeyboard.propTypes = {
    editableText: PropTypes.string.isRequired,
    closeKeyboard: PropTypes.func.isRequired,
    saveSegment: PropTypes.func.isRequired,
    cursorPosition: PropTypes.number.isRequired
};

export default TLKeyboard