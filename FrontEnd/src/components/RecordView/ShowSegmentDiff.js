import React from 'react'
import PropTypes from 'prop-types'
import {diffChars} from 'diff'

const ShowSegmentDiff = ({oldValue, newValue}) => {

    const diff = diffChars(oldValue, newValue);

    return (
        <span style={{lineHeight: 1.4}}>
            {diff.map((part, index) => (
                <span
                    key={index}
                    style={{
                        color: part.removed ? '#555' : '',
                        backgroundColor: part.added ? '#9e9' : part.removed ? '#e99' : '',
                        textDecoration: part.removed ? 'line-through' : '',
                        fontStyle: part.removed ? 'italic' : ''
                    }}
                    dangerouslySetInnerHTML={{__html: part.value}}
                ></span>
            ))}
        </span>
    )
};

ShowSegmentDiff.propTypes = {
    oldValue: PropTypes.string.isRequired,
    newValue: PropTypes.string.isRequired
};

export default ShowSegmentDiff