#!/bin/bash
set -e
PUBLISH_DIR="${1:-publish/osx-arm64}"
VOC_DEST="$PUBLISH_DIR/Dependencies/pc_nsf_hifigan_44.1k_hop512_128bin_2025.02"

echo "=== Downloading and bundling DiffSinger vocoder ==="
if [ -f "$VOC_DEST/vocoder.yaml" ]; then
    echo "Vocoder already bundled at $VOC_DEST, skipping."
    exit 0
fi

mkdir -p "$VOC_DEST"
gh release download pc-nsf-hifigan-44.1k-hop512-128bin-2025.02 \
    -R openvpi/vocoders \
    -p "*.oudep" \
    -D /tmp/ \
    --clobber

unzip -o "/tmp/pc_nsf_hifigan_44.1k_hop512_128bin_2025.02.oudep" -d "$VOC_DEST"
rm -f "/tmp/pc_nsf_hifigan_44.1k_hop512_128bin_2025.02.oudep"

echo "=== Vocoder bundled successfully ==="
ls -lh "$VOC_DEST/vocoder.yaml"
